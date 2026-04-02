using Camagru.Application.Contracts.Common;
using Camagru.Application.Contracts.Posts;
using Camagru.Application.Interfaces;
using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Camagru.Application.UseCases.Posts;

public class AddCommentUseCase
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateBuilder _templateBuilder;
    private readonly ILogger<AddCommentUseCase> _logger;

    public AddCommentUseCase(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ICommentRepository commentRepository,
        IEmailSender emailSender,
        IEmailTemplateBuilder templateBuilder,
        ILogger<AddCommentUseCase> logger)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _commentRepository = commentRepository;
        _emailSender = emailSender;
        _templateBuilder = templateBuilder;
        _logger = logger;
    }

    public async Task<ServiceResult<AddCommentResponse>> ExecuteAsync(AddCommentRequest request)
    {
        var content = request.Text.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return ServiceResult<AddCommentResponse>.Fail("Comment text is required");
        }

        if (content.Length > 2_000)
        {
            return ServiceResult<AddCommentResponse>.Fail("Comments must not exceed 2000 characters");
        }

        var post = await _postRepository.GetByIdWithDetailsAsync(request.PostId);
        if (post == null)
        {
            return ServiceResult<AddCommentResponse>.Fail("Post not found");
        }

        var commenter = await _userRepository.GetByIdAsync(request.UserId);
        if (commenter == null)
        {
            return ServiceResult<AddCommentResponse>.Fail("User not found");
        }

        var comment = new Comment
        {
            PostId = request.PostId,
            UserId = request.UserId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);

        string? warningMessage = null;

        if (post.UserId != commenter.Id && post.User.EmailNotificationsEnabled)
        {
            try
            {
                var emailBody = _templateBuilder.BuildCommentNotification(
                    post.User.Username,
                    commenter.Username,
                    content,
                    post.Id);

                await _emailSender.SendAsync(post.User.Email, "New comment on your Camagru post", emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send comment notification for post {PostId} to user {UserId}", post.Id, post.UserId);
                warningMessage = "Comment saved, but the author notification email could not be delivered.";
            }
        }

        return ServiceResult<AddCommentResponse>.Ok(new AddCommentResponse
        {
            CommentId = comment.Id,
            PostId = request.PostId,
            CommentCount = await _commentRepository.GetCountByPostIdAsync(request.PostId),
            WarningMessage = warningMessage
        });
    }
}
