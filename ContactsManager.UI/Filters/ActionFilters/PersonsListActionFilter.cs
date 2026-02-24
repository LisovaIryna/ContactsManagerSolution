using ContactsManager.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ContactsManager.Filters.ActionFilters;

public class PersonsListActionFilter : IActionFilter
{
    private readonly ILogger<PersonsListActionFilter> _logger;

    public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // To do: add after logic here
        _logger.LogInformation("{FilterName}.{MethodName}", nameof(PersonsListActionFilter), nameof(OnActionExecuted));

        PersonsController personsController = (PersonsController)context.Controller;

        IDictionary<string, object?>? parameters = (IDictionary<string, object?>?) context.HttpContext.Items["arguments"];

        if (parameters != null)
        {
            if (parameters.ContainsKey("searchBy"))
            {
                personsController.ViewData["CurrentsearchBy"] = Convert.ToString(parameters["searchBy"]);
            }

            if (parameters.ContainsKey("searchString"))
            {
                personsController.ViewData["CurrentsearchString"] = Convert.ToString(parameters["searchString"]);
            }

            if (parameters.ContainsKey("sortBy"))
            {
                personsController.ViewData["CurrentsortBy"] = Convert.ToString(parameters["sortBy"]);
            }
            else
            {
                personsController.ViewData["CurrentSortBy"] = nameof(PersonResponse.PersonName);
            }

            if (parameters.ContainsKey("sortOrder"))
            {
                personsController.ViewData["CurrentsortOrder"] = Convert.ToString(parameters["sortOrder"]);
            }
            else
            {
                personsController.ViewData["CurrentSortOrder"] = nameof(SortOrderOptions.ASC);
            }
        }

        personsController.ViewBag.SearchFields = new Dictionary<string, string>()
        {
            { nameof(PersonResponse.PersonName), "Person Name" },
            { nameof(PersonResponse.Email), "Email" },
            { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
            { nameof(PersonResponse.Gender), "Gender" },
            { nameof(PersonResponse.CountryID), "Country" },
            { nameof(PersonResponse.Address), "Address" },
        };
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Items["arguments"] = context.ActionArguments;

        // To do: add before logic here
        _logger.LogInformation("{FilterName}.{MethodName}", nameof(PersonsListActionFilter), nameof(OnActionExecuting));

        if (context.ActionArguments.ContainsKey("searchBy"))
        {
            string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);

            // validate the searchBy parameter value
            if (!string.IsNullOrEmpty(searchBy))
            {
                var searchByOptions = new List<string>()
                {
                    nameof(PersonResponse.PersonName),
                    nameof(PersonResponse.Email),
                    nameof(PersonResponse.DateOfBirth),
                    nameof(PersonResponse.Gender),
                    nameof(PersonResponse.CountryID),
                    nameof(PersonResponse.Address)
                };

                // reset the searchBy parameter value
                if (searchByOptions.Any(temp => temp == searchBy) == false)
                {
                    _logger.LogInformation("searchBy actual value {searchBy}", searchBy);
                    context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
                    _logger.LogInformation("searchBy updated value {searchBy}", context.ActionArguments["searchBy"]);
                }
            }
        }
    }
}
