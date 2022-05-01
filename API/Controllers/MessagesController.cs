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
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            // Get username.
            var username = User.GetUsername();

            // Check if username is equal to recipient username.
            if (username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You cannot send messages to yourself");

            // Get both users (reciever/sender).
            var sender = await _userRepository.GetUserByUsernameAsync(username);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            // Check if we have recipient.
            if (recipient == null) return NotFound();

            // Create new message.
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };
            // Add message through repository.
            _messageRepository.AddMessage(message);

            // Return Dto if saved.
            if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

            // Else return bad request
            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]
            MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, 
                messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();
            
            return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            // Get username.
            var username = User.GetUsername();
            // Get message.
            var message = await _messageRepository.GetMessage(id);
            // Check if message is from sender or recipient of message, if not no authorization.
            if (message.Sender.UserName != username && message.Recipient.UserName != username)
                return Unauthorized();
            // Check if Sender is current user and set sender deleted to true.
            if (message.Sender.UserName == username) message.SenderDeleted = true;
            // Check if Recipient is current user and set recipient deleted to true.
            if (message.Recipient.UserName == username) message.RecipientDeleted = true;
            // If both sender and recipient deleted is true then delete from server.
            if (message.SenderDeleted && message.RecipientDeleted) 
                _messageRepository.DeleteMessage(message);
            // Save all changes
            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}