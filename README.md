# Wikiled.Arff
C# version of ARFF file manager


Basic operation - create dataset and add document/record
```c#
 var data = ArffDataSet.CreateSimple("Test");
var item = data.AddDocument();
item.AddRecord("a");
```
