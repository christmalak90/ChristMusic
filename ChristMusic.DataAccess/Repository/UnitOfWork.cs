using ChristMusic.DataAccess.Data;
using ChristMusic.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChristMusic.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            CoverType = new CoverTypeRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            User = new UserRepository(_db);
            UserRole = new UserRoleRepository(_db);
            Role = new RoleRepository(_db);
            SP_Call = new SP_Call(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            OrderDetails = new OrderDetailsRepository(_db);
        }

        public ICategoryRepository Category { get; private set; }
        public ICoverTypeRepository CoverType { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IUserRepository User { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IOrderDetailsRepository OrderDetails { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }

        //Represent the list of roles in the database
        public IRoleRepository Role { get; private set; }
        //Represent the table that maps the roleId and userId in the database
        public IUserRoleRepository UserRole { get; private set; }

        public ISP_Call SP_Call { get; private set; }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
