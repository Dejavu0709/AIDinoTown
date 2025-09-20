using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserAvatar : AvatarBase
{
    public UserInfo3D UserInfo3D;
    public void ShowInfo(string content = null)
    {
        UserInfo3D.Show(content);
    }
    public void HideInfo()
    {
        UserInfo3D.Hide();
    }
}
