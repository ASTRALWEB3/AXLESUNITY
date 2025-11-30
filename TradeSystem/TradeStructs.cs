using Fusion;

// This struct travels over the network.
// It must implement INetworkStruct.
public struct NetworkedTradeItem : INetworkStruct
{
    // The ID of the item (e.g. "Sword", "Stone"). 
    // Max 16 chars is usually enough for IDs.
    public NetworkString<_16> ItemID;

    public int Quantity;

    // Is this a blockchain item (NFT) or a regular game item?
    public NetworkBool IsNft;
}