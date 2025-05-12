using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Mimeo.DynamicUI.Demo.Shared.Models;
using System.Text.Json;

namespace Mimeo.DynamicUI.Demo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private static readonly object inMemoryLock = new();
        private static readonly List<TestModel> inMemorySampleDatabase = [];

        static TestController()
        {
            for (int i = 0; i < 100; i++)
            {
                inMemorySampleDatabase.Add(new TestModel
                {
                    Id = Guid.NewGuid(),
                    Name = $"Item {i}",
                    Description = "A sample item",
                    HTML = "<p>Some HTML content</p>",
                    JSON = $"{{ \"number\": \"{i}\" }}",
                    Number = 7,
                    Decimal = 3.14m,
                    DateTimeUtc = DateTime.UtcNow,
                    DateTimeOffset = DateTimeOffset.Now,
                    Time = TimeSpan.FromHours(1),
                    SingleSelect = "option1",
                    MultiSelect = ["option1", "option2"],
                    Color = "#00FFFF",
                    Section = new TestModel.SubModelSimple
                    {
                        Property1 = "Section",
                        Property2 = 4
                    },
                    StringList = [
                        "Item 1",
                        "Item 2"
                    ],
                    SimpleModelList =
                    [
                        new TestModel.SubModelSimple
                        {
                            Property1 = "Sub 1",
                            Property2 = 1
                        },
                        new TestModel.SubModelSimple
                        {
                            Property1 = "Sub 2",
                            Property2 = 2
                        }
                    ],
                    AdvancedModelList =
                    [
                        new TestModel.SubModelAdvanced
                        {
                            Property1 = "Sub 1",
                            Property2 = 1,
                            SubList = ["Sub-sub 1", "Sub-sub 2"]
                        }
                    ],
                    Enabled = true
                });
            }
        }

        [HttpGet]
        public PagedResultsModel<TestModel> Get(ODataQueryOptions<TestModel> oDataQueryOptions, [FromQuery] string? search)
        {
            PagedResultsModel<TestModel> response;

            lock (inMemoryLock)
            {
                IQueryable<TestModel> query;

                if (!string.IsNullOrEmpty(search))
                {
                    // Attempt to simulate a general search that applies to all fields
                    query = inMemorySampleDatabase.Where(x => JsonSerializer.Serialize(x).Contains(search)).ToList().AsQueryable();
                }
                else
                {
                    query = inMemorySampleDatabase.AsQueryable();
                }

                var data = oDataQueryOptions.ApplyTo(query);
                var totalCount = ((IQueryable<TestModel>)oDataQueryOptions.ApplyTo(query, AllowedQueryOptions.Top | AllowedQueryOptions.Skip)).Count();

                var skip = oDataQueryOptions.Skip?.Value ?? 0;
                var take = oDataQueryOptions.Top?.Value ?? 100;

                var result = ((IEnumerable<TestModel>)data);
                response = new PagedResultsModel<TestModel>(totalCount, skip, take, result.ToList());
            }

            return response;
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            lock (inMemoryLock)
            {
                var model = inMemorySampleDatabase.FirstOrDefault(x => x.Id == id);
                if (model == null)
                {
                    return NotFound();
                }

                return Ok(model);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] TestModel model)
        {
            lock (inMemoryLock)
            {
                var existing = inMemorySampleDatabase.FirstOrDefault(x => x.Id == model.Id);
                if (existing == null)
                {
                    inMemorySampleDatabase.Add(model);
                }
                else
                {
                    // Upsert for compatibility with import/export
                    inMemorySampleDatabase.Remove(existing);
                    inMemorySampleDatabase.Add(model);
                }
            }

            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] TestModel model)
        {
            lock (inMemoryLock)
            {
                var existing = inMemorySampleDatabase.FirstOrDefault(x => x.Id == model.Id);
                if (existing == null)
                {
                    return NotFound();
                }

                inMemorySampleDatabase.Remove(existing);
                inMemorySampleDatabase.Add(model);

                return Ok();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            lock (inMemoryLock)
            {
                var existing = inMemorySampleDatabase.FirstOrDefault(x => x.Id == id);
                if (existing == null)
                {
                    return NotFound();
                }

                inMemorySampleDatabase.Remove(existing);

                return Ok();
            }
        }
    }
}
