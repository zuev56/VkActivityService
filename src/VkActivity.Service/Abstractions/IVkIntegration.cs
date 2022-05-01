﻿using VkActivity.Service.Models.VkApi;

namespace VkActivity.Service.Abstractions;

public interface IVkIntegration
{
    /// <param name="screenNames">User IDs or ScreenNames</param>
    Task<List<VkApiUser>> GetUsersWithActivityInfoAsync(string[] screenNames);

    /// <param name="screenNames">User IDs or ScreenNames</param>
    Task<List<VkApiUser>> GetUsersWithFullInfoAsync(string[] screenNames);
}