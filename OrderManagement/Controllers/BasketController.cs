using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Models.Business;
using OrderManagement.Models.Requests;
using OrderManagement.Services;

namespace OrderManagement.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class BasketController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;

        public BasketController(IOrderService orderService, ILogger<BasketController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        private ActionResult Manipulate(Action operation)
        {
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                if (ex is OrderNotFoundException
                    || ex is ProductNotFoundException
                    || ex is ProductAlreadyExistsException
                    || ex is QuantityException
                    || ex is ArgumentException)
                {
                    _logger.LogWarning("Bad request detected: {0}", ex.Message);
                    return BadRequest(ex);
                }
                _logger.LogError(ex, "Error occured while manipulating items in order.");
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            return Ok();
        }

        private ActionResult<object> Initialize(Func<object> operation)
        {
            object result;
            try
            {
                result = operation();
            }
            catch (Exception ex)
            {
                if (ex is CurrencyNotFoundException || ex is ArgumentException)
                {
                    _logger.LogWarning("Bad request detected: {0}", ex.Message);
                    return BadRequest(ex);
                }
                _logger.LogError(ex, "Error occured while initializing new order.");
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            var url = Url.Link("Default", new { Controller = "BasketController", Action = "Get", orderId = result }) ?? "http://localhost";
            return Created(url, result);
        }

        private string GetValidationMessages()
        {
            return String.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
        }

        [HttpGet()]
        public ActionResult<IEnumerable<Order>> Get()
        {
            var orders = _orderService.GetOrders();
            if (orders.Count() == 0)
                return NotFound();
            return orders.ToList();
        }

        [HttpGet("{orderId:int}")]
        public ActionResult<Order> Get(int orderId)
        {
            if (ModelState.IsValid)
            {
                var order = _orderService.GetOrder(orderId);
                if (order != null)
                    return order;
                else return NotFound();
            }

            return BadRequest(GetValidationMessages());
        }

        [HttpPost("[action]")]
        public ActionResult<object> InitializeSameAddress(InitializeSameAddressRequest request)
        {
            if (ModelState.IsValid)
                return Initialize(() => {
                    _logger.LogInformation("New order initialization using equal billing and shipping addresses.");
                    return new {orderId = _orderService.Initialize(request.Client, request.Address, request.PaymentMethod, request.CurrencyCode, request.DiscountRate) };
                });

            _logger.LogWarning("Validation errors detected: {0}", GetValidationMessages());
            return BadRequest(GetValidationMessages());
        }

        [HttpPost("[action]")]
        public ActionResult<object> InitializeSeparateAddresses(InitializeSeparateAddressesRequest request)
        {
            if (ModelState.IsValid)
                return Initialize(() => {
                    _logger.LogInformation("New order initialization using separate billing and shipping addresses.");
                    return new { orderId = _orderService.Initialize(request.Client, request.ShippingAddress, request.BillingAddress, request.PaymentMethod, request.CurrencyCode, request.DiscountRate) };
                });
            _logger.LogWarning("Validation errors detected: {0}", GetValidationMessages());
            return BadRequest(GetValidationMessages());
        }

        [HttpPost("{orderId:int}/[action]")]
        public ActionResult Add(int orderId, [FromBody] AddRequest request)
        {
            if (ModelState.IsValid)
                    return Manipulate(() => {
                    _logger.LogInformation("Adding {0} {1} to order {2}", request.ProductCode, request.Quantity, orderId);
                    _orderService.Add(orderId, request.ProductCode, request.Quantity);
                });
            return BadRequest(GetValidationMessages());
        }

        [HttpDelete("{orderId:int}/[action]/{productCode}")]
        public ActionResult Remove(int orderId, [FromBody] string productCode)
        {
            if (ModelState.IsValid)
                    return Manipulate(()=> {
                    _logger.LogInformation("Removing {0} from order {1}", productCode, orderId);
                    _orderService.Remove(orderId, productCode);
                });
            return BadRequest(GetValidationMessages());
        }

        [HttpPut("{orderId:int}/[action]/{productCode}")]
        public ActionResult SetQuantity(int orderId, string productCode, [FromBody] uint quantity)
        {
            if (ModelState.IsValid)
                return Manipulate(() => {
                    _logger.LogInformation("Setting quantity {0} for {1} in order {2}", quantity, productCode, orderId);
                    _orderService.SetQuantity(orderId, productCode, quantity);
                });
            return BadRequest(GetValidationMessages());
        }

        [HttpPost("{orderId:int}/[action]")]
        public ActionResult Complete(int orderId)
        {
            if (ModelState.IsValid)
                return Manipulate(() => {
                    _logger.LogInformation("Completing order: {orderId}", orderId);
                    _orderService.Complete(orderId);
                });
            return BadRequest(GetValidationMessages());
        }
    }
}