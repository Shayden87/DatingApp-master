using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // Summary:
    // Creates, saves, and returns Message DTO. 
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;            
        }
    
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]
            MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, 
                messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            // Get username.
            var username = User.GetUsername();
            // Get message.
            var message = await _unitOfWork.MessageRepository.GetMessage(id);
            // Check if message is from sender or recipient of message, if not no authorization.
            if (message.Sender.UserName != username && message.Recipient.UserName != username)
                return Unauthorized();
            // Check if Sender is current user and set sender deleted to true.
            if (message.Sender.UserName == username) message.SenderDeleted = true;
            // Check if Recipient is current user and set recipient deleted to true.
            if (message.Recipient.UserName == username) message.RecipientDeleted = true;
            // If both sender and recipient deleted is true then delete from server.
            if (message.SenderDeleted && message.RecipientDeleted) 
                _unitOfWork.MessageRepository.DeleteMessage(message);
            // Save all changes
            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}