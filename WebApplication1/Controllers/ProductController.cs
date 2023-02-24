using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Xml;
using System.Xml.Linq;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : Controller
    {
        private readonly IAmazonDynamoDB dynamoDB;
        public ProductController(IAmazonDynamoDB dynamoDB)
        {
            this.dynamoDB = dynamoDB;
        }

        [HttpGet("get")]
        public async Task<ProductId[]> Get()
        {
            try
            {
                var result = await dynamoDB.ScanAsync(new ScanRequest
                {
                    TableName = "products"
                });
                if (result != null && result.Items != null)
                {
                    var products = new List<ProductId>();
                    foreach (var item in result.Items)
                    {
                        item.TryGetValue("id", out var id);
                        item.TryGetValue("category", out var category);
                        item.TryGetValue("name", out var name);
                        item.TryGetValue("description", out var description);
                        item.TryGetValue("images", out var images);
                        item.TryGetValue("price", out var price);
                        products.Add(new ProductId(id?.S, name?.S, description?.S, category?.S, images?.SS,price?.S));
                    }
                    return products.ToArray();
                }
                return Array.Empty<ProductId>();
            }catch(Exception ex)
            {
                ProductId[] products = new ProductId[1];
                products[0] = new ProductId("error:", ex.Message, "", "",new List<string>(),"");
                return products;
            }
        }
        [HttpGet("get/{id}")]
        public async Task<ProductId> GetId([FromRoute] string id)
        {
            try
            {
                var request = new GetItemRequest
                {
                    TableName = "products",
                    Key = new Dictionary<string, AttributeValue>()
                    {
                        {"id", new AttributeValue(id)}
                    }
                };
                var result = await dynamoDB.GetItemAsync(request);
                result.Item.TryGetValue("id", out var idResponse);
                result.Item.TryGetValue("category", out var category);
                result.Item.TryGetValue("name", out var name);
                result.Item.TryGetValue("description", out var description);
                result.Item.TryGetValue("images", out var images);
                result.Item.TryGetValue("price", out var price);
                return new ProductId(idResponse?.S, name?.S, description?.S, category?.S, images?.SS, price?.S);
            }catch (Exception ex)
            {
                return new ProductId("error:", ex.Message, "", "", new List<string>(), "");
            }

        }

        [HttpPost]
        [Route("post")]
        public async Task<PutItemResponse> Post([FromBody] Product product)
        {
            string cadenaAleatoria = string.Empty;
            cadenaAleatoria = Guid.NewGuid().ToString();
            try
            {
                var request = new PutItemRequest
                {
                    TableName = "products",
                    Item = new Dictionary<string, AttributeValue>
                    {
                        {"id", new AttributeValue(cadenaAleatoria)},
                        {"category", new AttributeValue(product.category)},
                        {"name", new AttributeValue(product.name)},
                        {"description", new AttributeValue(product.description)},
                        {"images", new AttributeValue{SS = new List<string>(product.images)} },
                        {"price",  new AttributeValue(product.price) }
                    }
                };
               var Response=  await dynamoDB.PutItemAsync(request);
                return Response;
            }catch(Exception ex)
            {
                var data= new PutItemResponse();
                data.Attributes.Add("error", new AttributeValue(ex.Message));
                return data;
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<DeleteItemResponse> Delete(string id)
        {
            try
            {
                var request = new DeleteItemRequest
                {
                    TableName = "products",
                    Key = new Dictionary<string, AttributeValue>()
                    {
                        {"id", new AttributeValue(id)}
                    }
                };
                var result = await dynamoDB.DeleteItemAsync(request);
                return result;
            }catch(Exception ex)
            {
                var res = new DeleteItemResponse();
                res.Attributes.Add("error", new AttributeValue(ex.Message));
                return res;
            }
            
        }
        [HttpPut("put/{id}")]
        public async Task<UpdateItemResponse> Put(Product product,string id) {
            try
            {
                var request = new UpdateItemRequest
                {
                    TableName = "products",
                    Key = new Dictionary<string, AttributeValue>() { { "id", new AttributeValue(id) } },
                    AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
                    {
                        ["name"] = new AttributeValueUpdate
                        {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { S = product.name }
                        },
                        ["category"] = new AttributeValueUpdate
                        {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { S = product.category }
                        },
                        ["description"] = new AttributeValueUpdate
                        {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { S = product.description }
                        },
                        ["images"] = new AttributeValueUpdate
                        {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { SS = new List<string>(product.images)}
                        },
                        ["price"] = new AttributeValueUpdate
                        {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { S = product.price }
                        }
                    }
                };
                var res = await dynamoDB.UpdateItemAsync(request);
                return res;
            }
            catch(Exception ex)
            {
                var res = new UpdateItemResponse();
                res.Attributes.Add("error", new AttributeValue(ex.Message));
                return res;
            }
        }
    }
    public class Product
    {
        public Product(string name, string description, string category, List<string> images, string price)
        {
            this.name = name;
            this.description = description;
            this.category = category;
            this.images = images;
            this.price = price;
        }
        public List<string> images { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string category { get; set; }

        public string price { get; set; }
    }
    public class ProductId : Product
    {
        public ProductId(string id, string name, string description, string category, List<string> images, string price) :base(name,description,category,images,price)
        {
            this.id= id;
        }
        public string id { get; set; }

    }
}
