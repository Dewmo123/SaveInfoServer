﻿using DataAccessLayer.Repositories;

namespace Repositories;

public class RepositoryManager : IUnitOfWork
{
    private readonly string _dbAddress;

    private PlayerInfoRepository? _playerInfoRepo;
    private PlayerItemRepostory? _playerItemRepo;
    private AuctionItemRepository? _auctionInfoRepo;
    private ItemInfoRepository? _itemInfoRepo;
    private PlayerDataRepository? _playerGoldRepo;
    public RepositoryManager(string address)
    {
        _dbAddress = address;
    }

    public IPlayerDataRepository PlayerData => _playerGoldRepo ??= new PlayerDataRepository();

    public IPlayerInfoRepository PlayerInfos => _playerInfoRepo ??= new PlayerInfoRepository();

    public IItemInfoRepository ItemInfos => _itemInfoRepo ??= new ItemInfoRepository();

    public IPlayerItemRepository PlayerItems => _playerItemRepo ??= new PlayerItemRepostory();

    public IAuctionRepository AuctionItems => _auctionInfoRepo ??= new AuctionItemRepository();

    public void Dispose()
    {
    }
}