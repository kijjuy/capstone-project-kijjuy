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


    //TODO: THings to test:
    //Model with one prop, with column, should map one prop
    //Model with one prop, without column, should not map, maybe error (or warning log)?
    //Model with 0 props (should throw some error?)
    //Model with multiple props, all with column, should map all
    //Model with multiple props, some with column (should be valid, no exception/error)
    //Model with multiple props, no column (should still be valid, maybe log warning?)
    //Model with columns, but some data doesn't fill those columns (maybe throw exception... not sure yet)

    // Test Models

    //Should map single prop
    public class ModelWithSinglePropHasColumn {
	[Column("prop1")]
	public String prop1 { get; set; }
    }

    //Should log warning that there are no props to map
    public class ModelWithSinglePropNoColumn {
	public String prop1 { get; set; }
    }

    //Should throw exception
    public class ModelWithNoProps {

    }

    //Should Map all props
    public class ModelWithMultiplePropsAllHasColumn {
	[Column("strProp")]
	public String strProp { get; set; }
	[Column("intProp")]
	public int intProp { get; set; }
	[Column("dateTimeProp")]
	public DateTime dateTimeProp { get; set; }
	[Column("boolProp")]
	public bool boolProp { get; set; }
	[Column("floatProp")]
	public float floatProp { get; set; }
    }

    //Should map only props with column
    public class ModelWithMultiplePropsSomeHasColumn {
	public String strProp { get; set; }
	[Column("intProp")]
	public int intProp { get; set; }
	public DateTime dateTimeProp { get; set; }
	[Column("boolProp")]
	public bool boolProp { get; set; }
	public float floatProp { get; set; }
    }

    //Should not map anything, log warning
    public class ModelWithMultiplePropsNoColumn {
	public String strProp { get; set; }
	public int intProp { get; set; }
	public DateTime dateTimeProp { get; set; }
	public bool boolProp { get; set; }
	public float floatProp { get; set; }
    }

    //TODO: THings to test:
    //Model with one prop, with column, should map one prop
    //Model with one prop, without column, should not map, maybe error (or warning log)?
    //Model with 0 props (should throw some error?)
    //Model with multiple props, all with column, should map all
    //Model with multiple props, some with column (should be valid, no exception/error)
    //Model with multiple props, no column (should still be valid, maybe log warning?)
    //Model with x amount of column attr, data with y amount of fields, x>y all cols map, should map all




    // Mapper tests



    [Fact]
    public void MoreColumnsThanDataMapsAllColums() {
	//arrange
	var curDateTime = DateTime.Now;
	var model = new ModelWithMultiplePropsAllHasColumn {
	    strProp = "strProp",
	    intProp = 1,
	    dateTimeProp = curDateTime,
	    boolProp = false,
	    floatProp = 3.14f,
	};

	var dataDict = new Dictionary<String, object>();
	dataDict["strProp"] = model.strProp.Clone();
	dataDict["intProp"] = 1;
	dataDict["dateTimeProp"] = curDateTime.ToString();


	//act
    	var mappedModel = _mapper.MapDataToModel<ModelWithMultiplePropsAllHasColumn>(dataDict);

	//assert
	Assert.Equal(model.strProp, mappedModel.strProp);
	Assert.Equal(model.intProp, mappedModel.intProp);
	Assert.Equal(model.dateTimeProp, mappedModel.dateTimeProp);
	//TODO: refactor ReaderMapper to iterate through dict keys rather than props, this
	//ensures all sql data is mapped, or an exception is thrown.
    }


    //Model with x amount of column attr, data with y amount of fields, x<y, throws exception
    [Fact]
    public void MoreDataThanColumnsThrowsException() {
	//arrange
	var curDateTime = DateTime.Now;
	var model = new ModelWithMultiplePropsAllHasColumn {
	    strProp = "strProp",
	    intProp = 1,
	    dateTimeProp = curDateTime,
	    boolProp = false,
	    floatProp = 3.14f,
	};

	var dataDict = new Dictionary<String, object>();
	dataDict["strProp"] = model.strProp.Clone();
	dataDict["intProp"] = 1;
	dataDict["dateTimeProp"] = curDateTime.ToString();
	dataDict["boolProp"] = false;
	dataDict["floatProp"] = model.floatProp;
	dataDict["extraProp"] = 1;

	//act
    	//assert
	Assert.Throws<ArgumentException>(() => {
	    var mappedModel = _mapper.MapDataToModel<ModelWithMultiplePropsAllHasColumn>(dataDict);
	});
    }

    
    /**
     * <summary>
     * Model with x amount of column attr, data with x amount of fields but does not map all, throws exception 
     * </summary>
     */
    [Fact]
    public void NotAllDataMapsToColumnsThrowsException() {
	//arrange
	var model = new ModelWithMultiplePropsAllHasColumn {
	    strProp = "strProp",
	    intProp = 1,
	    dateTimeProp = DateTime.Now,
	    boolProp = false,
	    floatProp = 3.14f,
	};

	var dataDict = new Dictionary<String, object>();
	dataDict["strProp"] = model.strProp.Clone();
	dataDict["unmatchedProp1"] = 1;
	dataDict["dateTimeProp"] = model.dateTimeProp.ToString();
	dataDict["unmatchedProp2"] = false;
	dataDict["floatProp"] = model.floatProp;

	//act
	//assert
	Assert.Throws<KeyNotFoundException>(() => {
	    var mappedModel = _mapper.MapDataToModel<ModelWithMultiplePropsAllHasColumn>(dataDict);
	});
	

    }

    //OLD TESTS, Deprecated
    //[Fact]
    //public void TestReaderMapper()
    //{
    //    //Arrange
    //    int amountOfTestProducts = 10;
    //    var sqlData = genBaseProductSqlData(amountOfTestProducts);

    //    //Act
    //    var models = new List<ProductDataModel>();
    //    for(int i = 0; i < amountOfTestProducts; i ++) {
    //        var prodDataModel = _mapper.MapDataToModel<ProductDataModel>(sqlData[i]);
    //        models.Add(prodDataModel);
    //    }

    //    //Assert
    //    for(int i = 0; i < amountOfTestProducts; i++){
    //            testDictSameAsProduct(sqlData[i], models[i]);
    //    }
    //}


    //[Fact]
    //public void TestReaderMapperMissingData()
    //{
    //    //Arrange
    //    var sqlDict = new Dictionary<String, object>();
    //    sqlDict["product_id"] = (long)1;
    //    sqlDict["category_id"] = (long)0;
    //    sqlDict["name"] = "";
    //    sqlDict["price"] = (double)0;
    //    sqlDict["description"] = "";
    //    sqlDict["creation_date"] = "";
    //    sqlDict["update_date"] = "";
    //    sqlDict["is_available"] = (long)0;

    //    //Act
    //    var prodDataModel = _mapper.MapDataToModel<ProductDataModel>(sqlDict);

    //    //Assert
    //    testDictSameAsProduct(sqlDict, prodDataModel);
    //}


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
}

