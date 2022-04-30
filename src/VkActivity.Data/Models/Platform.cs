﻿namespace VkActivity.Data.Models;

// https://dev.vk.com/reference/objects/user#last_seen

public enum Platform
{
    Undefined = 0,
    MobileSiteVersion,
    IPhoneApp,
    IPadApp,
    AndroidApp,
    WindowsPhoneApp,
    Windows10App,
    FullSiteVersion
}
