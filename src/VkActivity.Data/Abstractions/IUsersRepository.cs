﻿using VkActivity.Data.Models;
using VkActivity.Data.Repositories;

namespace VkActivity.Data.Abstractions;

public interface IUsersRepository : IBaseRepository<User>
{
    Task<List<User>> FindAllWhereNameLikeValueAsync(string value, int? skip, int? take, CancellationToken cancellationToken = default);
    Task<List<User>> FindAllByIdsAsync(int[] userIds);
    Task<bool> SaveRangeAsync(IEnumerable<User> usersForSave, CancellationToken cancellationToken = default);
    //Task FindAllAsync(int[] userIds);
}
