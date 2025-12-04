using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace app.Repositories;

public interface IOrdersRepository
{
    public Task<int> CreatePendingOrder(
	String name,
	String address,
	double subtotal, 
	double tax, 
	String username
    );
    public Task CompleteOrder(int orderId, double shipping, double total);

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
    public Task<String> GetUsernameFromOrderId(int orderId);
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

    public async Task<int> CreatePendingOrder(
	String name, 
	String address,
	double subtotal, 
	double tax, 
	String username
    )
    {
	using var db = new SqliteConnection(_connString);
	using var query = new SqliteCommand(@"
	    INSERT INTO orders 
	    (shipping_name, shipping_address, subtotal_paid, tax_paid, user_name)
	    VALUES(@shipping_name, @shipping_address, @subtotal_paid, @tax_paid, @user_name)
	    RETURNING order_id; 
	", db);

	query.Parameters.AddWithValue("@shipping_name", name);
	query.Parameters.AddWithValue("@shipping_address", address);
	query.Parameters.AddWithValue("@subtotal_paid", subtotal);
	query.Parameters.AddWithValue("@tax_paid", tax);
	query.Parameters.AddWithValue("@user_name", username);

	await db.OpenAsync();
	using var reader = await query.ExecuteReaderAsync();
	int id = reader.GetInt32(0);

	return id;

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

    public async Task CompleteOrder(int orderId, double shipping, double total) 
    {
	using var db = new SqliteConnection(_connString);
	await db.OpenAsync();

	using var transaction = await db.BeginTransactionAsync() as SqliteTransaction;

	using (var command = db.CreateCommand()) {
	    command.CommandText = @"
		SELECT product_id FROM order_products
		WHERE order_id = @order_id;
		";

	    command.Transaction = transaction;
	    command.Parameters.AddWithValue("@order_id", orderId);

	    using var reader = await command.ExecuteReaderAsync();
	    while(await reader.ReadAsync()) 
	    {
		int productId = reader.GetInt32(0);
		using var updateCommand = db.CreateCommand();
		updateCommand.CommandText = @"
		    UPDATE products SET	is_available = 0
		    WHERE product_id = @product_id;
		    ";

		updateCommand.Transaction = transaction;
		updateCommand.Parameters.AddWithValue("@product_id", productId);

		int result = await updateCommand.ExecuteNonQueryAsync();

		if(result == 0) 
		{
		    throw new InvalidOperationException("Update product result was 0 when it should have been 1");
		}
	    }
	}

	using (var command = db.CreateCommand()) {
	    command.CommandText = @"
		UPDATE orders 
		SET shipping_paid = @shipping_paid,
		total_paid = @total_paid,
		order_status = 'complete'
		WHERE order_id = @order_id;
		";

	    command.Transaction = transaction;
	    command.Parameters.AddWithValue("@shipping_paid", shipping);
	    command.Parameters.AddWithValue("@total_paid", total);
	    command.Parameters.AddWithValue("@order_id", orderId);

	    int result = await command.ExecuteNonQueryAsync();

	    if(result == 0) {
		throw new InvalidOperationException("Update order reuslt was 0 when it should have been 1");
	    }

	}

	await transaction.CommitAsync();
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

    public async Task<String> GetUsernameFromOrderId(int orderId)
    {
	using var db = new SqliteConnection(_connString);
	await db.OpenAsync();
	using var query = new SqliteCommand(@"
		SELECT user_name FROM orders
		WHERE order_id = @order_id;
		", db);

	query.Parameters.AddWithValue("@order_id", orderId);

	using var reader = await query.ExecuteReaderAsync();
	await reader.ReadAsync();

	String username = reader.GetString(0);
	_logger.LogDebug($"Got username: {username} from orderid: {orderId}");

	return username;
    }
}
