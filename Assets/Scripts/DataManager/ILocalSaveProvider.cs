namespace RobotGame.Persistence
{
    /// <summary>
    /// Abstraction for local/cloud storage. Supports basic types and
    /// allows future back‑end integration without touching game code.
    /// </summary>
    public interface ILocalSaveProvider
    {
        /* ---------- Integer ---------- */
        void SetInt(string key, int value);
        int GetInt(string key, int defaultValue = 0);

        /* ---------- Float ---------- */
        void SetFloat(string key, float value);
        float GetFloat(string key, float defaultValue = 0f);

        /* ---------- String ---------- */
        void SetString(string key, string value);
        string GetString(string key, string defaultValue = "");

        /* ---------- Commit ---------- */
        void ResetAccount();
        void Save();
    }
}
