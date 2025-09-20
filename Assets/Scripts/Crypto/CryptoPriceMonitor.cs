using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using com.ootii.Messages;
using MiniJSON;

public class CryptoPriceMonitor : MonoBehaviour
{
    [Header("Symbols to monitor (Binance symbols)")]
    public List<string> symbols = new List<string> { "BTCUSDT", "ETHUSDT", "SOLUSDT", "XTERUSDT" };

    [Header("Polling interval (seconds)")]
    public float pollInterval = 20f;

    [Header("Alert threshold (%)")] 
    [Range(0.0f, 50f)]
    public float alertThresholdPercent = 0f;

    [Header("Alert cooldown per symbol (seconds)")]
    public float alertCooldown = 10f;

    private Dictionary<string, double> lastPrice = new Dictionary<string, double>();
    private Dictionary<string, double> lastAlertPrice = new Dictionary<string, double>();
    private Dictionary<string, float> lastAlertTime = new Dictionary<string, float>();
    private HashSet<string> invalidSymbols = new HashSet<string>();

    private const string BinanceUrlBase = "https://api.binance.com/api/v3/ticker/price?symbols=";
    private const string BinanceSingleUrlBase = "https://api.binance.com/api/v3/ticker/price?symbol=";

    private void Start()
    {
        // Trigger daily prices once per day on first run
        TrySendDailyPrices();
        
        // Start polling
        StartCoroutine(PollPricesLoop());
    }

    private void TrySendDailyPrices()
    {

        StartCoroutine(FetchAndSendDailyPrices());

    }

    private IEnumerator FetchAndSendDailyPrices()
    {
        yield return FetchPrices(result => {
            if (result != null && result.Count > 0)
            {
                Debug.Log("DailyCryptoPrices: " + result.Count);
                string msg = BuildDailyMessage(result);
                MessageDispatcher.SendMessageData("DailyCryptoPrices", msg);
            }
        });
    }

    private IEnumerator PollPricesLoop()
    {
        var wait = new WaitForSeconds(pollInterval);
        while (true)
        {
            yield return wait;
            yield return FetchPrices(OnPricesFetched);
        }
    }

    private IEnumerator FetchPrices(Action<Dictionary<string, double>> onDone)
    {
        // Filter out previously detected invalid symbols
        var activeSymbols = new List<string>();
        foreach (var s in symbols)
        {
            if (!invalidSymbols.Contains(s)) activeSymbols.Add(s);
        }
        if (activeSymbols.Count == 0)
        {
            Debug.LogWarning("No valid crypto symbols to fetch. Check configuration.");
            onDone?.Invoke(null);
            yield break;
        }
        Debug.Log("Fetching crypto prices...");
        // URL encode symbols array for Binance API
        string symbolsJson = JsonArray(activeSymbols);
        string url = BinanceUrlBase + UnityWebRequest.EscapeURL(symbolsJson);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.timeout = 10;
            yield return www.SendWebRequest();

            string text = www.downloadHandler.text;
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Crypto fetch error (batch): " + www.error + ", body: " + text);
                // If batch fails due to invalid symbol, fallback to per-symbol queries
                if (!string.IsNullOrEmpty(text) && text.Contains("Invalid symbol"))
                {
                    // Fall back to per-symbol and record invalid ones
                    yield return FetchPricesIndividually(onDone);
                    yield break;
                }
                else
                {
                    onDone?.Invoke(null);
                    yield break;
                }
            }
            Debug.Log("Crypto fetch success: " + text);
            // Parse JSON array of objects: [{"symbol":"BTCUSDT","price":"65000.00"}, ...]
            object parsed = Json.Deserialize(text);
            var dict = new Dictionary<string, double>();
            if (parsed is List<object> list)
            {
                foreach (var item in list)
                {
                    Debug.Log("Crypto fetch success: " + item);
                    if (item is Dictionary<string, object> row)
                    {
                        Debug.Log("2Crypto fetch success: " + row);
                        if (row.TryGetValue("symbol", out object symObj) && row.TryGetValue("price", out object priceObj))
                        {
                            Debug.Log("3Crypto fetch success: " + symObj + " " + priceObj);
                            string sym = symObj.ToString();
                            if (double.TryParse(priceObj.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double p))
                            {
                                dict[sym] = p;
                            }
                        }
                    }
                }
            }
            Debug.Log("Crypto fetch success: " + dict.Count);
            onDone?.Invoke(dict);
        }
    }

    private IEnumerator FetchPricesIndividually(Action<Dictionary<string, double>> onDone)
    {
        var dict = new Dictionary<string, double>();
        // Work with only currently active (non-invalid) symbols
        foreach (var sym in symbols)
        {
            string url = BinanceSingleUrlBase + UnityWebRequest.EscapeURL(sym);
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.timeout = 10;
                yield return www.SendWebRequest();

                string text = www.downloadHandler.text;
                if (www.result != UnityWebRequest.Result.Success)
                {
                    // Skip invalid symbol or network error
                    Debug.LogWarning($"Crypto fetch error for {sym}: {www.error}, body: {text}");
                    if (!string.IsNullOrEmpty(text) && text.Contains("Invalid symbol"))
                    {
                        invalidSymbols.Add(sym);
                    }
                    continue;
                }

                object parsed = Json.Deserialize(text);
                if (parsed is Dictionary<string, object> row)
                {
                    if (row.TryGetValue("symbol", out object symObj) && row.TryGetValue("price", out object priceObj))
                    {
                        if (double.TryParse(priceObj.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double p))
                        {
                            dict[symObj.ToString()] = p;
                        }
                    }
                }
            }
        }
        onDone?.Invoke(dict);
    }

    private void OnPricesFetched(Dictionary<string, double> prices)
    {
        Debug.Log("OnPricesFetched: " + prices.Count);
        if (prices == null || prices.Count == 0) return;

        // Thresholds
        float normalPct = Mathf.Max(0.00f, alertThresholdPercent);
        float severePct = Mathf.Max(normalPct * 2f, normalPct + 1f); // default severe > normal

        // Collect regular ticks and alert lines, and send only once per poll
        var tickEntries = new List<string>();
        var alertEntries = new List<string>();

        foreach (var kv in prices)
        {
            string sym = kv.Key;
            double price = kv.Value;

            if (lastPrice.TryGetValue(sym, out double prev))
            {
                if (prev > 0)
                {
                    double change = (price - prev) / prev * 100.0;
                    float absChange = Mathf.Abs((float)change);
                    Debug.Log("absChange: " + absChange);
                    Debug.Log("normalPct: " + normalPct);
                    if (absChange >= normalPct)
                    {
                        Debug.Log("absChange >= normalPct");
                        // Cooldown check
                        float now = Time.unscaledTime;
                        //if (!lastAlertTime.TryGetValue(sym, out float lastT) || (now - lastT) >= alertCooldown)
                        {
                            lastAlertTime[sym] = now;
                            lastAlertPrice[sym] = price;
                            bool isUp = change >= 0;
                            bool isSevere = absChange >= severePct;
                            string prefix;
                            if (isUp)
                                prefix = isSevere ? "【警报-大涨】" : "【提醒-上涨】";
                            else
                                prefix = isSevere ? "【警报-大跌】" : "【提醒-下跌】";

                            string sign = isUp ? "+" : "-";
                            double deltaAbs = price - prev;
                            string msg = $"{prefix}{SymToPretty(sym)} {sign}{absChange:F2}% 现价 {price:F2} USDT (前价 {prev:F2}, 变动 {deltaAbs:F2})";
                            alertEntries.Add(msg);
                            Debug.Log("alertEntries: " + alertEntries.Count);
                        }
                    }
                    else
                    {
                        // No large move, add to regular tick display
                        tickEntries.Add($"{SymToPretty(sym)} {price:F2} USDT");
                    }
                }
            }
            else
            {
                // First time seeing this symbol: include in regular tick so users see the price immediately
                tickEntries.Add($"{SymToPretty(sym)} {price:F2} USDT");
            }
            lastPrice[sym] = price;
        }

        // Build a single message per poll
        var outSb = new System.Text.StringBuilder();
        if (tickEntries.Count > 0)
        {
            outSb.AppendLine("最新价格:");
            for (int i = 0; i < tickEntries.Count; i++)
            {
                outSb.AppendLine("- " + tickEntries[i]);
            }
        }
        if (alertEntries.Count > 0)
        {
            if (outSb.Length > 0) outSb.AppendLine();
            outSb.AppendLine("波动提示:");
            for (int i = 0; i < alertEntries.Count; i++)
            {
                outSb.AppendLine("- " + alertEntries[i]);
            }
        }

        // Send only once if we have any content
        if (outSb.Length > 0)
        {
            MessageDispatcher.SendMessageData("CryptoTick", outSb.ToString());
        }
    }

    private string BuildDailyMessage(Dictionary<string, double> prices)
    {
        // Only include selected main symbols mapping
        var wanted = new List<string> { "BTCUSDT", "ETHUSDT", "SOLUSDT"};
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("今日开机价:");
        foreach (var sym in wanted)
        {
            if (prices.TryGetValue(sym, out double p))
            {
                Debug.Log($"- {SymToPretty(sym)}: {p:F2} USDT");
                sb.AppendLine($"- {SymToPretty(sym)}: {p:F2} USDT");
            }
            else
            {
                Debug.Log($"- {SymToPretty(sym)}: 暂无数据");
                sb.AppendLine($"- {SymToPretty(sym)}: 暂无数据");
            }
        }
        Debug.Log($"sb.ToString():" + sb.ToString());
        return sb.ToString();
    }

    private static string JsonArray(List<string> items)
    {
        // Build ["A","B"] without needing a full serializer
        var sb = new System.Text.StringBuilder();
        sb.Append('[');
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append('"').Append(items[i]).Append('"');
        }
        sb.Append(']');
        return sb.ToString();
    }

    private string SymToPretty(string sym)
    {
        if (sym.EndsWith("USDT")) sym = sym.Substring(0, sym.Length - 4);
        switch (sym)
        {
            case "BTC": return "BTC";
            case "ETH": return "ETH";
            case "SOL": return "SOL";
            case "XTER": return "XTER";
            default: return sym;
        }
    }
}
