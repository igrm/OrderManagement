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
    /// <summary>  
    ///  Class implements of Basket API.
    ///  Exposes service endoints.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class BasketController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;

        /// <summary>
        /// BasketController constructor.
        /// </summary>
        /// <param name="orderService">IOrderService implementation interface implementation</param>
        /// <param name="logger">Logger implementation interface implementation</param>
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

        /// <summary>
        /// Get a list of initialized orders.
        /// </summary>
        [HttpGet()]
        public ActionResult<IEnumerable<Order>> Get()
        {
            var orders = _orderService.GetOrders();
            if (orders.Count() == 0)
                return NotFound();
            return orders.ToList();
        }

        /// <summary>
        /// Get order details.
        /// </summary>
        /// <param name="orderId">Identifier of initialized order.</param>
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

        /// <summary>
        /// Initializes the order using the same address for both billing and shipping addresses.
        /// </summary>
        /// <param name="request">Initialization request with filled data.</param>
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

        /// <summary>
        /// Initializes the order using separate billing and shipping addresses.
        /// </summary>
        /// <param name="request">Initialization request with filled data.</param>
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

        /// <summary>
        /// Add a product to an order.
        /// </summary>
        /// <param name="orderId">Identifier of initialized order.</param>
        /// <param name="request">Request for adding product to the order.</param>
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

        /// <summary>
        /// Remove product from an order
        /// </summary>
        /// <param name="orderId">Identifier of initialized order.</param>
        /// <param name="productCode">Product code to remove.</param>
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

        /// <summary>
        /// Sets the quantity for product in order.
        /// </summary>
        /// <param name="orderId">Identifier of initialized order.</param>
        /// <param name="productCode">Product code to remove.</param>
        /// <param name="quantity">Number of product items.</param>
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

        /// <summary>
        /// Complete the order.
        /// </summary>
        /// <param name="orderId">Identifier of initialized order.</param>
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

        /// <summary>
        /// Clear out order data.
        /// </summary>
        /// <param name="orderId">Identifier of initialized order.</param>
        [HttpDelete("{orderId:int}/[action]")]
        public ActionResult ClearOut(int orderId)
        {
            if (ModelState.IsValid)
                return Manipulate(() => {
                    _logger.LogInformation("Clearing out items from order: {orderId}", orderId);
                    _orderService.ClearOut(orderId);
                });
            return BadRequest(GetValidationMessages());
        }
    }
}