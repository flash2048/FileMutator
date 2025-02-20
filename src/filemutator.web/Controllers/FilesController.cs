using AutoMapper;
using FileMutator.infrastructure.EF;
using FileMutator.infrastructure.EF.Entities;
using FileMutator.infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileMutator.Tools.Interfaces;

namespace FileMutator.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly FileMutatorDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMutatorService _mutator;

    public FilesController(FileMutatorDbContext dbContext, IMapper mapper, IMutatorService mutator)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mutator = mutator;
    }

    [EndpointSummary("Uploads a file and stores its metadata.")]
    [EndpointDescription("Accepts a file, validates it, and saves metadata in the database. Returns file details on success or an error message if the file is missing.")]
    [ProducesResponseType(typeof(FileInfoShort), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [HttpPost("upload")]
    public async Task<ActionResult<FileInfoShort>> UploadFileAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        var newFile = _mapper.Map<FileEntity>(file);

        _dbContext.Files.Add(newFile);
        await _dbContext.SaveChangesAsync();

        return Ok(_mapper.Map<FileInfoShort>(newFile));
    }

    [EndpointSummary("Mutates an existing file by modifying its content.")]
    [EndpointDescription("Finds a file by its ID, applies a mutation to its content, updates metadata, and saves changes. Returns the updated file details or a 404 error if the file is not found.")]
    [ProducesResponseType(typeof(FileInfoFull), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [HttpPost("mutate/{fileId}")]
    public async Task<ActionResult<FileInfoFull>> MutateFile(Guid fileId)
    {
        var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId);
        if (file == null)
            return NotFound("File not found.");

        file.FileData = _mutator.MutateText(file.FileData);
        file.IsMutated = true;
        file.Size = file.FileData.Length;
        await _dbContext.SaveChangesAsync();

        return Ok(_mapper.Map<FileInfoFull>(file));
    }

    [EndpointSummary("Downloads a file by its ID.")]
    [EndpointDescription("Retrieves a file from the database and returns it as a binary stream. Returns an error if the file is not found.")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(Stream), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadFileAsync(Guid fileId)
    {
        var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId);
        if (file == null)
            return NotFound("File not found.");

        return File(file.FileData, file.ContentType, file.Name);
    }

    [EndpointSummary("Retrieves file information by its ID.")]
    [EndpointDescription("Fetches file details from the database, including its name, size, and mutation status. Returns an error if the file is not found.")]
    [ProducesResponseType(typeof(FileInfoFull), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [HttpGet("{fileId}")]
    public async Task<ActionResult<FileInfoFull>> GetFileInfoAsync(Guid fileId)
    {
        var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId);
        if (file == null)
            return NotFound("File not found.");

        return Ok(_mapper.Map<FileInfoFull>(file));
    }
}
