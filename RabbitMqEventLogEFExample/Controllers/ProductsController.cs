using System.Linq;
using System.Threading.Tasks;
using EventBus;
using EventBus.EventLog;
using Microsoft.AspNetCore.Mvc;

namespace RabbitMqEventLogEFExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IPersistentEventTransaction _persistentEventTransaction;
        private readonly IEventLogPublisher _eventLogPublisher;
        private readonly IEventLogService _eventLogService;

        public ProductsController(ApplicationContext  applicationContext, IPersistentEventTransaction persistentEventTransaction, IEventLogPublisher eventLogPublisher, IEventLogService eventLogService)
        {
            _applicationContext = applicationContext;
            _persistentEventTransaction = persistentEventTransaction;
            _eventLogPublisher = eventLogPublisher;
            _eventLogService = eventLogService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Product product)
        {
            AddProduct(product);    
            var depositIncreased = IncreasesProductDepositCount();
            if (!depositIncreased)
                return new BadRequestObjectResult(new { message = "The deposit can only handle a maximum number of 2 products" });

            var transactionId = await _persistentEventTransaction.SaveChangesWithEventLogsAsync();
            await _eventLogPublisher.PublishPendingEventLogs(transactionId);
            return Ok(new { message = "New product added" });
        }

        private bool IncreasesProductDepositCount()
        {
            var deposit = _applicationContext.Deposits.FirstOrDefault() ?? new Deposit();

            if (deposit.ProductCount == 2000)
            {
                return false;
            }

            var oldCount = deposit.ProductCount;
            deposit.ProductCount++;

            if (_applicationContext.Deposits.Local.Any(e => e.Id == deposit.Id))
                _applicationContext.Deposits.Update(deposit);
            else
                _applicationContext.Deposits.Add(deposit);

            _eventLogService.AddEvent(new DepositProductCountChandedEvent(deposit.ProductCount, oldCount));
            return true;
        }

        private void AddProduct(Product product)
        {
            _applicationContext.Products.Add(product);
            for (int i = 0; i < 50; i++)
            {
                _eventLogService.AddEvent(new NewProductAddedEvent(product));
            }
        }
    }

    public class NewProductAddedEvent : Event
    {
        public NewProductAddedEvent(Product product)
        {
            Product = product;
        }

        public Product Product { get; set; }
    }

    public class DepositProductCountChandedEvent : Event
    {
        public DepositProductCountChandedEvent(int newCount, int oldCount)
        {
            NewCount = newCount;
            OldCount = oldCount;
        }
        public int NewCount { get; set; }
        public int OldCount { get; set; }
    }
}