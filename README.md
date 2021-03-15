# COVID-19 Top Global Cases
ASP.NET Core solution that give us the top 10 of regions/provinces with most COVID-19 cases around the world.

## This solution is build with:
* **RESTful API:** ASP NET Core v3.1 MVC Web Application that consume *[Rapid API (COVID-19 Statistics)](https://rapidapi.com/axisbits-axisbits-default/api/covid-19-statistics)* services which obtain the COVID-19 global data. 
* **Web Application:** ASP NET Core v3.1 MVC Web Application which is devoloped with C# and JQuery/Bootstrap.
* **XUnitTestProject:** build with *XUnit* to test the RESTful API services.
* **ClassLibrary:** A class library project that stores all the models used in all the projects of the solution.

## Principal functions of the Web Application:
### **SEARCH BY ALL REGIONS**
The application will return the TOP 10 of regions with most COVID-19 cases.

![image](https://user-images.githubusercontent.com/20680805/111094180-50619a00-8500-11eb-8263-9dbeb5cc0c91.png)

### **SEARCH BY SPECIFIC REGION**
The application will return the TOP 10 of provinces with most COVID-19 cases, for this you must select a specific region.

![image](https://user-images.githubusercontent.com/20680805/111094357-c239e380-8500-11eb-9546-f9baf75a4386.png)

### **EXPORT DATA TO XML, JSON OR CSV**
You can export the data reflected in the grid by clicking in any of these buttons:

![image](https://user-images.githubusercontent.com/20680805/111094730-80f60380-8501-11eb-98e5-0d7be7f7c869.png)

The application will download the file of your choice.

![image](https://user-images.githubusercontent.com/20680805/111094845-c1558180-8501-11eb-82e6-bbd1b4181067.png)

If you want to see some samples of the exported files, you can see this *[wwwroot/export](https://github.com/carlosgodinez24/Covid-19TopCases/tree/main/Covid19TopCases/wwwroot/export)* directory.
