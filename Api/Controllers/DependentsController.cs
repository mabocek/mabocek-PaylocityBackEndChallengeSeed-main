/*
using Api.Dtos.Dependent;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DependentsController : ControllerBase
{
    [SwaggerOperation(Summary = "Get dependent by id")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetDependentDto>>> Get(int id)
    {
        // Simulate async operation
        await Task.Delay(1);

        // Get all dependents first
        var dependents = await GetAllDependentsData();

        // Find the dependent by id
        var dependent = dependents.FirstOrDefault(d => d.Id == id);

        if (dependent == null)
        {
            return NotFound(new ApiResponse<GetDependentDto>
            {
                Data = null,
                Success = false,
                Message = $"Dependent with id {id} not found"
            });
        }

        var result = new ApiResponse<GetDependentDto>
        {
            Data = dependent,
            Success = true
        };

        return Ok(result);
    }

    [SwaggerOperation(Summary = "Get all dependents")]
    [HttpGet("")]
    public async Task<ActionResult<ApiResponse<List<GetDependentDto>>>> GetAll()
    {
        // Simulate async operation
        await Task.Delay(1);

        var dependents = await GetAllDependentsData();

        var result = new ApiResponse<List<GetDependentDto>>
        {
            Data = dependents,
            Success = true
        };

        return Ok(result);
    }

    private async Task<List<GetDependentDto>> GetAllDependentsData()
    {
        // Simulate async operation
        await Task.Delay(1);

        //task: use a more realistic production approach
        var dependents = new List<GetDependentDto>
        {
            new()
            {
                Id = 1,
                FirstName = "Spouse",
                LastName = "Morant",
                Relationship = Relationship.Spouse,
                DateOfBirth = new DateOnly(1998, 3, 3)
            },
            new()
            {
                Id = 2,
                FirstName = "Child1",
                LastName = "Morant",
                Relationship = Relationship.Child,
                DateOfBirth = new DateOnly(2020, 6, 23)
            },
            new()
            {
                Id = 3,
                FirstName = "Child2",
                LastName = "Morant",
                Relationship = Relationship.Child,
                DateOfBirth = new DateOnly(2021, 5, 18)
            },
            new()
            {
                Id = 4,
                FirstName = "DP",
                LastName = "Jordan",
                Relationship = Relationship.DomesticPartner,
                DateOfBirth = new DateOnly(1974, 1, 2)
            }
        };

        return dependents;
    }
}
*/
