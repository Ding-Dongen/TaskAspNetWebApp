using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Business.ViewModel;
using X.PagedList.Extensions;


namespace TaskAspNet.Web.Controllers;

public class ClientController(IClientService clientService, ILogger<ClientController> logger) : Controller
{
    private readonly IClientService _clientService = clientService;

    private readonly ILogger<ClientController> _logger = logger;


    public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
    {
        try
        {
            var clients = await _clientService.GetAllClientsAsync();

            var paged = clients.ToPagedList(page, pageSize);   

            var viewModel = new ClientIndexViewModel
            {
                AllMembers = clients,        
                CreateClient = new ClientDto(),
                PagedClients = paged          
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Client Index page.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
    
    public IActionResult Create()
    {
        try
        {
            return View(new ClientDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering Create Client page.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient(ClientDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.Addresses ??= new List<MemberAddressDto>();
                model.Phones ??= new List<MemberPhoneDto>();
                return View("Create", model);
            }

            await _clientService.CreateClientAsync(model);
            TempData["SuccessMessage"] = "Client Added";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client.");
            TempData["ErrorMessage"] = "An unexpected error occurred while adding the client.";
            return View("Create", model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null) return NotFound();

            return PartialView("~/Views/Shared/Partials/Components/Client/_CreateEditClient.cshtml", client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Edit form for client {ClientId}.", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClientDto model)
    {
        try
        {
            if (!ModelState.IsValid)
                return PartialView("~/Views/Shared/Partials/Components/Client/_CreateEditClient.cshtml", model);

            await _clientService.UpdateClientAsync(model);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client {ClientId}.", model.Id);
            TempData["ErrorMessage"] = "An unexpected error occurred while updating the client.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
            {
                TempData["ErrorMessage"] = "Client not found or already deleted.";
            }
            else
            {
                await _clientService.DeleteClientAsync(id);
                TempData["SuccessMessage"] = $"Deleted client: {client.ClientName}";
            }
        }
        catch (DbUpdateException ex) when
            (ex.InnerException?.Message.Contains("DELETE statement conflicted with the REFERENCE constraint") == true)
        {
            _logger.LogWarning(ex, "Delete blocked by FK constraint for client {ClientId}.", id);
            TempData["ErrorMessage"] = "Client could not be deleted. Reassign or delete the client’s projects first.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting client {ClientId}.", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the client.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Dropdown")]
    public async Task<IActionResult> Dropdown()
    {
        try
        {
            var clients = await _clientService.GetAllClientsAsync();
            return PartialView("~/Views/Shared/Partials/Components/Client/_ClientSelect.cshtml", clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading client dropdown.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}
