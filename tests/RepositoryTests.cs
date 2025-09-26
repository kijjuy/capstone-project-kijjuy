using app.Models;
using app.Repositories;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;

namespace tests;

public class RepositoryTests
{
    private readonly ReaderMapper _mapper;

    public RepositoryTests() 
    {
	var loggerFactory = LoggerFactory.Create(builder => {
	    builder.AddConsole();
	});
	ILogger<ReaderMapper> logger = loggerFactory.CreateLogger<ReaderMapper>();
	_mapper = new ReaderMapper(logger);
    }


    private List<Dictionary<String, object>> genBaseProductSqlData(int numToGen)
    {
        List<Dictionary<String, object>> sqlDataProducts = new List<Dictionary<String, object>>();
        var props = typeof(Product).GetProperties();
        for (int i = 0; i < numToGen; i++)
        {
            var newDict = new Dictionary<String, object>();
            newDict["product_id"] = (long)i;
            newDict["category_id"] = (long)i % 3;
            newDict["name"] = $"Product{i}";
            newDict["price"] = 100.00 + i;
            newDict["description"] = $"description for Product{i}";
            newDict["creation_date"] = DateTime.Now.ToString();
            newDict["update_date"] = DateTime.Now.ToString();
            newDict["is_available"] = (long)1%2;
	    sqlDataProducts.Add(newDict);
        }
	return sqlDataProducts;
    }

    private void testDictSameAsProduct(Dictionary<String, object> dict, ProductDataModel prod)  
    {
	foreach(var prop in typeof(ProductDataModel).GetProperties())
	{
	    var colName = prop.GetCustomAttribute<ColumnAttribute>();
	    var propVal = prop.GetValue(prod);
	    var dictVal = dict[colName.Name];
	    if (prop.PropertyType.Equals(typeof(DateTime))) {
		//ugly, had to do this to get empty sql string to match new DateTime
		if(dictVal.Equals(String.Empty)) {
		    dictVal = new DateTime().ToString();
		}
		dictVal = DateTime.Parse((String)dictVal);
	    }
	    Assert.Equal(prop.GetValue(prod), dictVal);
	}
    }


    [Fact]
    public void TestReaderMapper()
    {
        //Arrange
	int amountOfTestProducts = 10;
	var sqlData = genBaseProductSqlData(amountOfTestProducts);

        //Act
	var models = new List<ProductDataModel>();
	for(int i = 0; i < amountOfTestProducts; i ++) {
	    var prodDataModel = _mapper.MapDataToModel<ProductDataModel>(sqlData[i]);
	    models.Add(prodDataModel);
	}

        //Assert
	for(int i = 0; i < amountOfTestProducts; i++){
	        testDictSameAsProduct(sqlData[i], models[i]);
	}
    }


    [Fact]
    public void TestReaderMapperMissingData()
    {
        //Arrange
	var sqlDict = new Dictionary<String, object>();
	sqlDict["product_id"] = (long)1;
        sqlDict["category_id"] = (long)0;
        sqlDict["name"] = "";
        sqlDict["price"] = (double)0;
        sqlDict["description"] = "";
        sqlDict["creation_date"] = "";
        sqlDict["update_date"] = "";
        sqlDict["is_available"] = (long)0;

        //Act
	var prodDataModel = _mapper.MapDataToModel<ProductDataModel>(sqlDict);

        //Assert
	testDictSameAsProduct(sqlDict, prodDataModel);
    }
}

