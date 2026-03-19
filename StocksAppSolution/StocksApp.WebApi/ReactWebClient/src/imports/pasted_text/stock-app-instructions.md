Instructions
Submission
Instructor example
Give feedback
Assignment instructions
60 minutes to complete
113 student solutions
Requirement
Create an Asp.Net Core Web Application that displays stock price with live updates from "https://finnhub.io/".





UI Design
Trade/Index:




Trade/Orders:




Trade/OrdersPDF:








Architecture
Use the n-layer (also known as n-tier) architecture as shown below.






Finnhub.io:
https://finnhub.io is a service provider that provides live stock price information online.



User-Secrets:
Register in "https://finnhub.io/login" to generate your own token [or] use the token "cc676uaad3i9rj8tb1s0" to make requests.

After registration at finnhub, you can find your API Key (token) at "https://finnhub.io/dashboard".

You need to store this token in user-secrets on your machine.

You need to attach the token as a part of request url while making requests to "finnhub.io".





appsettings:
You will store the following configuration in appsettings.json:

  "TradingOptions":
  {
    "DefaultStockSymbol": "MSFT",
    "DefaultOrderQuantity": 100
  },
  "ConnectionStrings":
  {
    "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=YourDatabaseHere;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
  }


The "DefaultStockSymbol" acts as a default stock to fetch details and display stock price.

The "DefaultOrderQuantity" specifies default quantity to place an order in "Trade/Index" view.

The "DefaultConnection" represents database connection details.





Entity model:
You need to create two entity model classes named "BuyOrder" and "SellOrder".



1. Consider an entity model class called 'BuyOrder' with following properties, along with corresponding validations:

Guid BuyOrderID

string StockSymbol [Mandatory]

string StockName [Mandatory]

DateTime DateAndTimeOfOrder

uint Quantity [Value should be between 1 and 100000]

double Price [Value should be between 1 and 10000]



2. Consider another entity model class called 'SellOrder' with following properties, along with corresponding validations:

Guid SellOrderID

string StockSymbol [Mandatory]

string StockName [Mandatory]

DateTime DateAndTimeOfOrder

uint Quantity [Range(1, 100000)]

double Price [Range(1, 10000)]



These two entity classes are used to create list objects to store records in a service class.





DbContext:
You need to create a DbContext called "StockMarketDbContext" that connects to the database using the connection string mentioned in the appsettings.json.

It contains the following DbSets'.

DbSet<BuyOrder> BuyOrders

DbSet<SellOrder> SellOrders





DTO:
We are implementing the functionality of placing (creating) buy order and sell order.

So you need to create four DTO model classes named "BuyOrderRequest", "BuyOrderResponse", "SellOrderRequest", "SellOrderResponse".

As you can guess, the Request model classes (such as BuyOrderRequest and SellOrderRequest) are used as parameters in the service methods to receive information from controller to service class. Alternatively, the Response model classes (such as BuyOrderResponse and SellOrderResponse) are used as return type to return information back to controller, from the service.



1. Consider a DTO model class called 'BuyOrderRequest' with following properties, along with corresponding validations:

string StockSymbol [Mandatory]

string StockName [Mandatory]

DateTime DateAndTimeOfOrder [Should not be older than Jan 01, 2000]

uint Quantity [Value should be between 1 and 100000]

double Price [Value should be between 1 and 10000]



2. Consider another DTO model class called 'SellOrderRequest' with following properties, along with corresponding validations:

string StockSymbol [Mandatory]

string StockName [Mandatory]

DateTime DateAndTimeOfOrder [Should not be older than Jan 01, 2000]

uint Quantity [Value should be between 1 and 100000]

double Price [Value should be between 1 and 10000]



3. Consider another DTO model class called 'BuyOrderResponse' with following properties:

Guid BuyOrderID

string StockSymbol

string StockName

DateTime DateAndTimeOfOrder

uint Quantity

double Price

double TradeAmount



4. Consider another DTO model class called 'SellOrderResponse' with following properties:

Guid SellOrderID

string StockSymbol

string StockName

DateTime DateAndTimeOfOrder

uint Quantity

double Price

double TradeAmount





FinnhubService:


Create a service interface called 'IFinnhubService' with following methods:

Task<Dictionary<string, object>?> GetCompanyProfile(string stockSymbol);

Task<Dictionary<string, object>?> GetStockPriceQuote(string stockSymbol);



Implement the above service interface called 'IFinnhubService' that sends request to the respective url and returns its response as a Dictionary<string, object>.

GetCompanyProfile: https://finnhub.io/api/v1/stock/profile2?symbol={symbol}&token={token}

GetStockPriceQuote: https://finnhub.io/api/v1/quote?symbol={symbol}&token={token}



The service methods such as GetCompanyProfile() and GetStockPriceQuote() return the response data that was actually returned by finnhub.io/api.





IFinnhubService.GetStockPriceQuote():
The returned data from IFinnhubServiceGetStockPriceQuote() looks like this:

{"c":235.87,"d":9.12,"dp":4.0221,"h":236.6,"l":226.06,"o":226.24,"pc":226.75,"t":1666987204}



Response Attributes:

c

Current price



d

Change



dp

Percent change



h

High price of the day



l

Low price of the day



o

Open price of the day



pc

Previous close price





Reference: https://finnhub.io/docs/api/quote





IFinnhubService.GetCompanyProfile():
The returned data from IFinnhubService.GetCompanyProfile() looks like this:

{"country":"US","currency":"USD","exchange":"NASDAQ NMS - GLOBAL MARKET","finnhubIndustry":"Technology","ipo":"1986-03-13","logo":"https://static2.finnhub.io/file/publicdatany/finnhubimage/stock_logo/MSFT.svg","marketCapitalization":1758286.5806001066,"name":"Microsoft Corp","phone":"14258828080.0","shareOutstanding":7454.47,"ticker":"MSFT","weburl":"https://www.microsoft.com/en-us"}



Response Attributes:

country

Country of company's headquarter.



currency

Currency used in company filings.



exchange

Listed exchange.



finnhubIndustry

Finnhub industry classification.



ipo

IPO date.



logo

Logo image.



marketCapitalization

Market Capitalization.



name

Company name.



phone

Company phone number.



shareOutstanding

Number of oustanding shares.



ticker

Company symbol/ticker as used on the listed exchange.



weburl

Company website.



Reference: https://finnhub.io/docs/api/company-profile2







StocksService:
Create a service interface called 'IStocksService' with following methods:

Task<BuyOrderResponse> CreateBuyOrder(BuyOrderRequest? buyOrderRequest);

Task<SellOrderResponse> CreateSellOrder(SellOrderRequest? sellOrderRequest);

Task<List<BuyOrderResponse>> GetBuyOrders();

Task<List<SellOrderResponse>> GetSellOrders();



Implement the above service interface called 'IStocksService' that performs the specified operation.

CreateBuyOrder: Inserts a new buy order into the database table called 'BuyOrders'.

CreateSellOrder: Inserts a new sell order into the database table called 'SellOrders'.

GetBuyOrders: Returns the existing list of buy orders retrieved from database table called 'BuyOrders'.

GetSellOrders: Returns the existing list of sell orders retrieved from database table called 'SellOrders'.







StockTrade:
You need to create two view model classes called "StockTrade" and "Orders".

Create a view model class called "StockTrade" in the Asp.Net Core project with following properties:

string? StockSymbol

string? StockName

double Price

unit Quantity



This class will be used to send model object from controller to the "Trade/Index" view.







Orders:
Create a view model class called "Orders" in the Asp.Net Core project with following properties:

List<BuyOrderResponse> BuyOrders

List<SellOrderResponse> SellOrders



This class will be used to send list of buy orders and sell orders - from controller to the "Trade/Orders" view.







"Trade\Index.cshtml":
You need to create three views called "Trade\Index.cshtml", "Trade\Orders.cshtml" and "Trade\OrdersPDF.cshtml".



Create a view called "Index.cshtml" in "Views\Trade" folder - that is strongly typed to the view model class called "StockTrade". So it receives the model object of "StockTrade" type and displays the stock name and its price as shown in below screenshot.




The view should store the stock symbol (Eg: MSFT) as a hidden element. So that, it can be accessible in the javascript code, while connecting finnhub websocket.

So, you need to write javascript code to connect to finnhub websocket at `wss://ws.finnhub.io?token=${token}` url. On receiving each message from the socket, you need update the latest stock price in the UI.

The response from finnhub websocket looks like this:

   {"data":[{"p":220.89,"s":"MSFT","t":1575526691134,"v":100}, {"p":220.82,"s":"MSFT","t":1575526691167,"v":15}],"type":"trade"}



 Response attributes:

    type: message type

    data: [ list of trades ]

    s: symbol of the company

    p: Last price

    t: UNIX milliseconds timestamp

    v: volume (number of orders)

    c: trade conditions (if any)



You can see more than one element in the 'data' array in the above response example. But in the output, we need to display only one. You can display either of the prices (or) the highest price in the output.



When the page is closed, you must unsubscribe from the finnhub websocket to avoid any memory leaks.



Overall, your application should refresh the updated price of the stock for every one or two seconds as long as the page runs (of course, only when the market is LIVE i.e. usually 09:30 a.m. to 4 p.m. (ET)).



When the user clicks on the "Buy" button, the browser should make a HTTP POST request to "Trade/BuyOrder" with values of "StockSymbol", "StockName", "Quantity" and "Price". You can submit values that are not to be directly displayed, through input type="hidden" fields also.



When the user clicks on the "Sell" button, the browser should make a HTTP POST request to "Trade/SellOrder" with values of "StockSymbol", "StockName", "Quantity" and "Price". You can submit values that are not to be directly displayed, through input type="hidden" fields also.





"Trade\Orders.cshtml":
Create a view called "Orders.cshtml" in "Views\Trade" folder - that is strongly typed to the view model class called "Orders". So it receives the model object of "Orders" type from controller and displays the list of both buy orders and sell orders as shown in the below screenshot.




"Trade\OrdersPDF.cshtml":
Create a view called "OrdersPDF.cshtml" in "Views\Trade" folder - that is strongly typed view bound to list of orders. So it receives list of both BuyOrder and SellOrder from controller & displays the list or orders in printable format as shown in the below screenshot.










Trade Controller:
Create a controller called TradeController that has five action methods called "Index()", "BuyOrder()" and "SellOrder()", "Orders()" and "OrdersPDF()".

The controller has to inject the appsettings called "TradingOptions" (from appsettings.json), IFinnhubService, IStocksService.





TradeController.Index():
- It receives HTTP GET request at route "Trade/Index".

- It first gets the "TradingOptions" from appsettings.json using Options pattern.



- The Index() action method invokes the following methods:

FinnhubService.GetCompanyProfile() to fetch stock name, stock symbol and other details.

FinnhubService.GetStockPriceQuote() to fetch current stock price.



- And then it creates an object of "StockTrade" model class and fills essential data such as StockSymbol, StockName, Price and Quantity that are read from the return values of above mentioned service methods i.e. "FinnhubService.GetCompanyProfile()" and "FinnhubService.GetStockPriceQuote()".

- Then it sends the same model object to the view.





TradeController.BuyOrder():
It receives HTTP POST request at route "Trade/BuyOrder".

It receives the model object of "BuyOrder" type through model binding.

It initializes "DateAndTimeOfOrder" into the model object (i.e. buyOrder).

If model state has no errors, it invokes StocksService.CreateBuyOrder() method. Then it redirects to "Trade/Orders" route to display list of orders.

Alternatively, in case of validation errors in the model object, it reinvokes the same view, along with same model object.





TradeController.SellOrder():
It receives HTTP POST request at route "Trade/SellOrder".

It receives the model object of "SellOrder" type through model binding.

It initializes "DateAndTimeOfOrder" into the model object (i.e. sellOrder).

If model state has no errors, it invokes StocksService.CreateSellOrder() method. Then it redirects to "Trade/Orders" route to display list of orders.

Alternatively, in case of validation errors in the model object, it reinvokes the same view, along with same model object.





TradeController.Orders():
It receives HTTP GET request at route "Trade/Orders".

It invokes both the service methods StocksService.GetBuyOrders() and StocksService.GetSellOrders().

Then it creates an object of the view model class called 'Orders' and initializes both 'BuyOrders' and 'SellOrders' properties with the data returned by the above called service methods.

It invokes the "Trade/Orders" view to display list of orders.





TradeController.OrdersPDF():
It receives HTTP GET request at route "Trade/OrdersPDF".

It invokes both the service methods StocksService.GetBuyOrders() and StocksService.GetSellOrders().

It invokes the "Trade/OrdersPDF" view and renders it as PDF file using 'Rotativa' package.









xUnit Test cases:
Write the following unit test cases for testing respective service methods.



StocksService.CreateBuyOrder():

1. When you supply BuyOrderRequest as null, it should throw ArgumentNullException.

2. When you supply buyOrderQuantity as 0 (as per the specification, minimum is 1), it should throw ArgumentException.

3. When you supply buyOrderQuantity as 100001 (as per the specification, maximum is 100000), it should throw ArgumentException.

4. When you supply buyOrderPrice as 0 (as per the specification, minimum is 1), it should throw ArgumentException.

5. When you supply buyOrderPrice as 10001 (as per the specification, maximum is 10000), it should throw ArgumentException.

6. When you supply stock symbol=null (as per the specification, stock symbol can't be null), it should throw ArgumentException.

7. When you supply dateAndTimeOfOrder as "1999-12-31" (YYYY-MM-DD) - (as per the specification, it should be equal or newer date than 2000-01-01), it should throw ArgumentException.

8. If you supply all valid values, it should be successful and return an object of BuyOrderResponse type with auto-generated BuyOrderID (guid).



StocksService.CreateSellOrder():

1. When you supply SellOrderRequest as null, it should throw ArgumentNullException.

2. When you supply sellOrderQuantity as 0 (as per the specification, minimum is 1), it should throw ArgumentException.

3. When you supply sellOrderQuantity as 100001 (as per the specification, maximum is 100000), it should throw ArgumentException.

4. When you supply sellOrderPrice as 0 (as per the specification, minimum is 1), it should throw ArgumentException.

5. When you supply sellOrderPrice as 10001 (as per the specification, maximum is 10000), it should throw ArgumentException.

6. When you supply stock symbol=null (as per the specification, stock symbol can't be null), it should throw ArgumentException.

7. When you supply dateAndTimeOfOrder as "1999-12-31" (YYYY-MM-DD) - (as per the specification, it should be equal or newer date than 2000-01-01), it should throw ArgumentException.

8. If you supply all valid values, it should be successful and return an object of SellOrderResponse type with auto-generated SellOrderID (guid).





StocksService.GetAllBuyOrders():

1. When you invoke this method, by default, the returned list should be empty.

2. When you first add few buy orders using CreateBuyOrder() method; and then invoke GetAllBuyOrders() method; the returned list should contain all the same buy orders.





StocksService.GetAllSellOrders():

1. When you invoke this method, by default, the returned list should be empty.

2. When you first add few sell orders using CreateSellOrder() method; and then invoke GetAllSellOrders() method; the returned list should contain all the same sell orders.







Instructions:
Create controller(s) with attribute routing.

Provide the configuration as service, using Options pattern.

Inject the IOptions in the controller.

Use CSS styles, layout views, _ViewImports, _ViewStart as per the necessity.

The CSS file is provided as downlodable resource for applying essential styles. You can download and use it.

Inject IStocksService and IFinnhubService in Controller.

Invoke essential service methods in controller.

The Entity model class (BuyOrder and SellOrder) should not be accessed in the controller. They must be used only in the service classes.

The DTO model classes (BuyOrderRequest, BuyOrderResponse, SellOrderRequest, SellOrderResponse) should be used as parameter type or return type in the service methods; and can be used in both services and controller.

Use appropriate tag helpers such as "asp-controller", "asp-action", "asp-for" etc., in all views wherever necessary.





Changes from previous assignment:
Creating StockMarketDbContext and connecting to SQL Server database to store buy orders and sell orders.

"Trade/OrdersPDF" view that displays list of orders as PDF format.



Questions for this assignment
Check your source code with Instructor's source code.

Download resource files