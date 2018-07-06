using Microsoft.AspNetCore.Mvc.Testing;
using OrderManagement.Models;
using OrderManagement.Models.Business;
using OrderManagement.Models.Requests;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace OrderManagement.Tests
{
    public class IntegrationTests : IClassFixture<CustomWebApplicationFactory<OrderManagement.Startup>>
    {
        private readonly CustomWebApplicationFactory<OrderManagement.Startup> _factory;

        public IntegrationTests(CustomWebApplicationFactory<OrderManagement.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetBasket_Test()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("api/v1/basket");
            Assert.Equal((int)response.StatusCode, StatusCodes.Status200OK);
        }

        [Fact]
        public async Task OrderLifecycle_Test()
        {
            var client = _factory.CreateClient();

            //1. initialize order by same shipping and billing address
            var response = await client.PostAsJsonAsync("api/v1/basket/InitializeSameAddress"
                                                 , new InitializeSameAddressRequest()
                                                 {
                                                      Client = new Client()
                                                      {
                                                          FirstName = "John",
                                                          LastName = "Doe",
                                                          ClientCode = "4829",
                                                          BirthDate = DateTime.Now.Date.AddDays(-9600),
                                                          Gender = Gender.Male,
                                                          Contacts = new List<Contact>()
                                                          {
                                                              new Contact()
                                                              {
                                                                  ContactType = ContactType.Email,
                                                                  Value = "test@test.test"
                                                              }
                                                          }
                                                      },
                                                      Address = new Address()
                                                              {  Country="DE",
                                                                 City ="Berlin",
                                                                 AddressLine = "Wittenauer Straße",
                                                                 State = "Berlin zzz",
                                                                 Zip = "10115"
                                                              },
                                                      CurrencyCode="EUR",
                                                      DiscountRate=0.1m
                                                 }
            );
            string result = await response.Content.ReadAsStringAsync();

            if ((int)response.StatusCode == StatusCodes.Status201Created)
            {
                dynamic orderData = JsonConvert.DeserializeObject(result);

                await client.PostAsJsonAsync($"api/v1/basket/{orderData.orderId}/Add", new AddRequest() { ProductCode = "PRODUCT-1", Quantity = 2 });
                await client.PostAsJsonAsync($"api/v1/basket/{orderData.orderId}/Add", new AddRequest() { ProductCode = "PRODUCT-2", Quantity = 3 });
                await client.PutAsJsonAsync($"api/v1/basket/{orderData.orderId}/SetQuantity/PRODUCT-2", 2 );

                var basketResponse = await client.GetAsync($"api/v1/basket/{orderData.orderId}");

                if((int)basketResponse.StatusCode == StatusCodes.Status200OK)
                {
                    var temp = await basketResponse.Content.ReadAsStringAsync();
                    var order = JsonConvert.DeserializeObject<Order>(temp);
                    Assert.Equal(600, order.OrderAmount);
                }
                else Assert.True(false, "Order not found.");
            }
            else Assert.True(false, "Order not created.");
        }

   }
}
