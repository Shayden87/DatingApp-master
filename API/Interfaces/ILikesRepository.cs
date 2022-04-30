using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        // Get an individual Like.
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId); 
        // Get a user with their likes and include them.
        Task<AppUser> GetUserWithLikes(int userId);
        // Go and get the likes for a user.
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
        // Return DTO.
    }
}