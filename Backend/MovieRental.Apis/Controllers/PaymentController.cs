using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Payments;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles rental payment records and ledger queries.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff,Customer")] // Customer can view their own payments
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // Gets a paginated list of payments with amount and date filters.
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaymentQueryParametersDto queryParams)
        {
            var result = await _paymentService.GetAllPaymentsAsync(queryParams);
            return Ok(result);
        }

        // Gets payment details by ID.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            if (result == null) return NotFound($"Payment {id} not found");
            return Ok(result);
        }

        // Creates a new payment for a rental (Admin and Staff only).
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
        {
            try
            {
                var result = await _paymentService.CreatePaymentAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
