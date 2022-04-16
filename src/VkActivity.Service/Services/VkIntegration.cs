﻿using VkActivity.Service.Abstractions;
using VkActivity.Service.Models;
using Zs.Common.Services.WebAPI;

namespace VkActivity.Service.Services;

public class VkIntegration : IVkIntegration
{
    private readonly string _getUsersUrl;
    private const string FIELDS_FOR_GETTING_ACTIVITY = "online,online_mobile,online_app,last_seen";
    private const string FIELDS_FOR_ADDING_USER = "photo_id,verified,sex,bdate,city,country,home_town,photo_max_orig,online,domain,has_mobile,"
                    + "contacts,site,education,universities,schools,status,last_seen,followers_count,occupation,nickname,relatives,"
                    + "relation,personal,connections,exports,activities,interests,music,movies,tv,books,games,about,quotes,can_post,"
                    + "can_see_all_posts,can_see_audio,can_write_private_message,can_send_friend_request,is_favorite,is_hidden_from_feed,"
                    + "timezone,screen_name,maiden_name,is_friend,friend_status,career,military,blacklisted,blacklisted_by_me,can_be_invited_group";

    public VkIntegration(string token, string version)
    {
        ArgumentNullException.ThrowIfNull(nameof(token));
        ArgumentNullException.ThrowIfNull(nameof(version));

        _getUsersUrl = $"https://api.vk.com/method/users.get?access_token={token}&v={version}";
    }

    public async Task<VkApiResponse> GetUsersActivity(int[] userIds)
    {
        var url = $"{_getUsersUrl}&fields={FIELDS_FOR_GETTING_ACTIVITY}&user_ids={string.Join(',', userIds)}";

        return await ApiHelper.GetAsync<VkApiResponse>(url, throwExceptionOnError: true);
    }

    public async Task<VkApiResponse> GetUsers(int[] userIds)
    {
        var url = $"{_getUsersUrl}&fields={FIELDS_FOR_ADDING_USER}&user_ids={string.Join(',', userIds)}";

        return await ApiHelper.GetAsync<VkApiResponse>(url, throwExceptionOnError: true);
    }
}