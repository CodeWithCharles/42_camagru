using Camagru.Application.Contracts.Common;
using Camagru.Application.Contracts.Posts;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Posts;

public class GetAvailableOverlaysUseCase
{
    private readonly IOverlayRepository _overlayRepository;

    public GetAvailableOverlaysUseCase(IOverlayRepository overlayRepository)
    {
        _overlayRepository = overlayRepository;
    }

    public async Task<ServiceResult<List<OverlayDto>>> ExecuteAsync()
    {
        var overlays = await _overlayRepository.GetAllAsync();

        var dtos = overlays.Select(o => new OverlayDto
        {
            Id = o.Id,
            Name = o.Name,
            Category = o.Category,
            FilePath = o.FilePath,
            DisplayOrder = o.DisplayOrder
        }).ToList();

        return ServiceResult<List<OverlayDto>>.Ok(dtos);
    }
}
