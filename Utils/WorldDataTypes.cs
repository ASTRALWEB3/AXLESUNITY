using System;
using System.Collections.Generic;

[Serializable]
public class ApiWorldItem
{
    public string id;
    public string name;
    public string owner;
    public string purchasePrice;
    public string purchasedAt;
    public string transactionHash;
}

[Serializable]
public class ApiWorldData
{
    public List<ApiWorldItem> items;
    public int totalCount;
}

[Serializable]
public class DataWrapper
{
    public ApiWorldData worlds;
}

[Serializable]
public class GraphQLResponse
{
    public DataWrapper data;
}