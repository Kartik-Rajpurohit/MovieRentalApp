using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Payments;
using MovieRental.Repository.Permissions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles rental payment records and ledger queries.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Permissions.Payments.Read)] // Customers can view payment records; only Admin/Staff can record payments
    public class PaymentController : ControllerBase
    {
        // Injected service for payment ledger logic
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // Gets a paginated list of payments with amount, customer, staff, and date filters.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationInputDto pagination,
            [FromQuery] PaymentFilterDto filter)
        {
            var result = await _paymentService.GetAllPaymentsAsync(pagination, filter);
            return Ok(result);
        }

        // Gets a single payment detail by PaymentId.
        // Returns 404 NotFound if payment does not exist.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            if (result == null) return NotFound($"Payment {id} not found");
            return Ok(result);
        }

        // Records a new payment transaction against a returned rental.
        // Restricted to Payments.Create permission.
        // Returns 400 BadRequest if payment amount is invalid or rental is not eligible.
        [HttpPost]
        [Authorize(Policy = Permissions.Payments.Create)]
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
