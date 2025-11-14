using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace app.Repositories;

public interface IOrdersRepository
{
    public Task<int> AddOrder(
        String userName,
        double subtotal,
        double tax,
        double shipping,
        double total,
        String shippingAddress,
        String shippingName,
        String ccLast4,
        DateTime orderDate
        );
    public Task AddOrderProduct(int orderId, long productId);
}

public class OrdersRepository : IOrdersRepository
{
    private readonly String _connString;
    private readonly ILogger<OrdersRepository> _logger;

    public OrdersRepository(
        IOptions<RepositoryOptions> options,
        ILogger<OrdersRepository> logger
        )
    {
        _connString = options.Value.ConnectionString;
        _logger = logger;
    }

    public async Task<int> AddOrder(
        String userName,
        double subtotal,
        double tax,
        double shipping,
        double total,
        String shippingAddress,
        String shippingName,
        String ccLast4,
        DateTime orderDate
        )
    {
        using var db = new SqliteConnection(_connString);

        using var query = new SqliteCommand(@"
	    INSERT INTO orders 
		(user_name, subtotal_paid, tax_paid, shipping_paid, total_paid, shipping_address, shipping_name, cc_last_4, order_date)
		VALUES(@user_name, @subtotal_paid, @tax_paid, @shipping_paid, @total_paid, @shipping_address, @shipping_name, @cc_last_4, @order_date)
		RETURNING order_id;
		", db);

        query.Parameters.AddWithValue("@user_name", userName);
        query.Parameters.AddWithValue("@subtotal_paid", subtotal);
        query.Parameters.AddWithValue("@tax_paid", tax);
        query.Parameters.AddWithValue("@shipping_paid", shipping);
        query.Parameters.AddWithValue("@total_paid", total);
        query.Parameters.AddWithValue("@shipping_address", shippingAddress);
        query.Parameters.AddWithValue("@shipping_name", shippingName);
        query.Parameters.AddWithValue("@cc_last_4", ccLast4);
        query.Parameters.AddWithValue("@order_date", orderDate);

        foreach (SqliteParameter param in query.Parameters)
        {
            _logger.LogDebug($"ParamName = {param.ParameterName}; Value = {param.Value}");
        }

        await db.OpenAsync();

        using var reader = await query.ExecuteReaderAsync();

        reader.Read();
        int result = reader.GetInt32(0);

        _logger.LogInformation($"Created new order with id={result}");

        return result;
    }

    public async Task AddOrderProduct(int orderId, long productId)
    {
        using var db = new SqliteConnection(_connString);
        using var query = new SqliteCommand(@"
		INSERT INTO order_products
		    (product_id, order_id)
		    VALUES(@product_id, @order_id);
		", db);

        query.Parameters.AddWithValue("@product_id", productId);
        query.Parameters.AddWithValue("@order_id", orderId);

        await query.Connection!.OpenAsync();
        var result = await query.ExecuteNonQueryAsync();
    }
}
