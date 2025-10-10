using app.Repositories;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;

namespace tests;

public class RepositoryTests
{
    private readonly ReaderMapper _mapper;
    private const String constDateTimeStr = "2025-10-10T10:12:45.0000000";

    public RepositoryTests() 
    {
	var loggerFactory = LoggerFactory.Create(builder => {
	    builder.AddConsole();
	});
	ILogger<ReaderMapper> logger = loggerFactory.CreateLogger<ReaderMapper>();
	_mapper = new ReaderMapper(logger);
    }

    #region test models

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

    #endregion test models

    #region mapper tests

    // Mapper tests


    //Model with 0 props (should throw some error?)
    [Fact]
    public void ModelWithNoPropsThrowsError() {
	//arrange
	var dataDict = new Dictionary<String, object>();

	//act
	//assert
	Assert.Throws<ArgumentException>(() => {
	    _mapper.MapDataToModel<ModelWithNoProps>(dataDict);
		});
    }

    //Model with multiple props, all with column, should map all
    [Fact]
    public void MultiplePropsAllHasColumnAllMaps() {
	//arrange
	var model = new ModelWithMultiplePropsAllHasColumn {
	    strProp = "strProp",
	    intProp = 1,
	    dateTimeProp = DateTime.Parse(constDateTimeStr),
	    boolProp = false,
	    floatProp = 3.14f,
	};

	var dataDict = new Dictionary<String, object>();
	dataDict["strProp"] = model.strProp;
	dataDict["intProp"] = model.intProp;
	dataDict["dateTimeProp"] = constDateTimeStr;
	dataDict["boolProp"] = model.boolProp;
	dataDict["floatProp"] = model.floatProp;

	//act
	var mappedModel = _mapper.MapDataToModel<ModelWithMultiplePropsAllHasColumn>(dataDict);

	//assert
	Assert.Equal(mappedModel.strProp, model.strProp);
	Assert.Equal(mappedModel.intProp, model.intProp);
	Assert.Equal(mappedModel.dateTimeProp, model.dateTimeProp);
	Assert.Equal(mappedModel.boolProp, model.boolProp);
	Assert.Equal(mappedModel.floatProp, model.floatProp);
    }

    //Model with multiple props, some with column (should be valid, no exception/error)
    [Fact]
    public void MultiplePropsSomeHasColumnMapsSome() {
	//arrange
	var model = new ModelWithMultiplePropsSomeHasColumn {
	    strProp = "strProp",
	    intProp = 1,
	    dateTimeProp = DateTime.Parse(constDateTimeStr),
	    boolProp = false,
	    floatProp = 3.14f,
	};

	var dataDict = new Dictionary<String, object>();
	dataDict["intProp"] = model.intProp;
	dataDict["boolProp"] = model.boolProp;

	//act
	Console.WriteLine("starting mapping of ModelWithMultiplePropsSomeHasColumn");
	var mappedModel = _mapper.MapDataToModel<ModelWithMultiplePropsSomeHasColumn>(dataDict);
	Console.WriteLine("finished mapping of ModelWithMultiplePropsSomeHasColumn");

	//assert
	Assert.Equal(mappedModel.strProp, null);
	Assert.Equal(mappedModel.intProp, model.intProp);
	Assert.Equal(mappedModel.dateTimeProp, new DateTime());
	Assert.Equal(mappedModel.boolProp, model.boolProp);
	Assert.Equal(mappedModel.floatProp, 0f);
    }

    //Model with multiple props, no column (should still be valid, maybe log warning?)
    [Fact]
    public void MultiplePropsNoneHasColumnMapsNothing() {
	//arrange

	var dataDict = new Dictionary<String, object>();

	//act
	var mappedModel = _mapper.MapDataToModel<ModelWithMultiplePropsNoColumn>(dataDict);
	
	//assert
	Assert.Equal(mappedModel.strProp, null);
	Assert.Equal(mappedModel.intProp, 0);
	Assert.Equal(mappedModel.dateTimeProp, new DateTime());
	Assert.Equal(mappedModel.boolProp, false);
	Assert.Equal(mappedModel.floatProp, 0f);

    }

    //Model with x amount of column attr, data with y amount of fields, x>y all cols map, should map all
    [Fact]
    public void MoreColumnsThanDataMapsAllColums() {
	//arrange
	var model = new ModelWithMultiplePropsAllHasColumn {
	    strProp = "strProp",
	    intProp = 1,
	    dateTimeProp = DateTime.Parse(constDateTimeStr),
	    boolProp = false,
	    floatProp = 3.14f,
	};

	var dataDict = new Dictionary<String, object>();
	dataDict["strProp"] = model.strProp.Clone();
	dataDict["intProp"] = 1;
	dataDict["dateTimeProp"] = constDateTimeStr;


	//act
    	var mappedModel = _mapper.MapDataToModel<ModelWithMultiplePropsAllHasColumn>(dataDict);

	//assert
	Assert.Equal(model.strProp, mappedModel.strProp);
	Assert.Equal(model.intProp, mappedModel.intProp);
	Assert.Equal(model.dateTimeProp, mappedModel.dateTimeProp);
    }


    //Model with x amount of column attr, data with y amount of fields, x<y, throws exception
    [Fact]
    public void MoreDataThanColumnsThrowsException() {
	//arrange
	var model = new ModelWithMultiplePropsAllHasColumn {
	    strProp = "strProp",
	    intProp = 1,
	    dateTimeProp = DateTime.Parse(constDateTimeStr),
	    boolProp = false,
	    floatProp = 3.14f,
	};

	var dataDict = new Dictionary<String, object>();
	dataDict["strProp"] = model.strProp.Clone();
	dataDict["intProp"] = 1;
	dataDict["dateTimeProp"] = constDateTimeStr;
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
	    dateTimeProp = DateTime.Parse(constDateTimeStr),
	    boolProp = false,
	    floatProp = 3.14f,
	};

	var dataDict = new Dictionary<String, object>();
	dataDict["strProp"] = model.strProp.Clone();
	dataDict["unmatchedProp1"] = 1;
	dataDict["dateTimeProp"] = constDateTimeStr;
	dataDict["unmatchedProp2"] = false;
	dataDict["floatProp"] = model.floatProp;

	//act
	//assert
	Assert.Throws<ArgumentException>(() => {
	    var mappedModel = _mapper.MapDataToModel<ModelWithMultiplePropsAllHasColumn>(dataDict);
	});
	

    }
    #endregion mapper tests
}

