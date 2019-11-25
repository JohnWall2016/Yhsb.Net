using System;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Yhsb.Jb.Database
{
    public class FpData
    {
        [Column("序号"), Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NO { get; set; }

        [Column("乡镇街")]
        public string Xzj { get; set; }

        [Column("村社区")]
        public string Csq { get; set; }

        [Column("地址")]
        public string Address { get; set; }

        [Column("姓名")]
        public string Name { get; set; }

        [Key, Column("身份证号码")]
        public string IDCard { get; set; }

        [Column("出生日期")]
        public string BirthDay { get; set; }

        [Column("贫困人口")]
        public string Pkrk { get; set; }

        [Column("贫困人口日期")]
        public string PkrkDate { get; set; }

        [Column("特困人员")]
        public string Tkry { get; set; }

        [Column("特困人员日期")]
        public string TkryDate { get; set; }
        
        [Column("全额低保人员")]
        public string Qedb { get; set; }
        
        [Column("全额低保人员日期")]
        public string QedbDate { get; set; }
        
        [Column("差额低保人员")]
        public string Cedb { get; set; }
        
        [Column("差额低保人员日期")]
        public string CedbDate { get; set; }

        [Column("一二级残疾人员")]
        public string Yejc { get; set; }

        [Column("一二级残疾人员日期")]
        public string YejcDate { get; set; }

        [Column("三四级残疾人员")]
        public string Ssjc { get; set; }

        [Column("三四级残疾人员日期")]
        public string SsjcDate { get; set; }
        
        [Column("属于贫困人员")]
        public string Sypkry { get; set; }
        
        [Column("居保认定身份")]
        public string Jbrdsf { get; set; }
        
        [Column("居保认定身份最初日期")]
        public string JbrdsfFirstDate { get; set; }
        
        [Column("居保认定身份最后日期")]
        public string JbrdsfLastDate { get; set; }

        [Column("居保参保情况")]
        public string Jbcbqk { get; set; }

        [Column("居保参保情况日期")]
        public string JbcbqkDate { get; set; }
    }

    public class FpRawData
    {
        [Column("序号"), Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NO { get; set; }

        [Column("乡镇街")]
        public string Xzj { get; set; }

        [Column("村社区")]
        public string Csq { get; set; }

        [Column("地址")]
        public string Address { get; set; }

        [Column("姓名")]
        public string Name { get; set; }

        [Key, Column("身份证号码")]
        public string IDCard { get; set; }

        [Column("出生日期")]
        public string BirthDay { get; set; }

        [Column("人员类型")]
        public string Type { get; set; }

        [Column("类型细节")]
        public string Detail { get; set; }

        [Column("数据月份")]
        public string Date { get; set; }
    }

    public class FpDbContext : DbContext
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder) => 
                optionsBuilder.UseMySql(_internal.Database.DBConnectString);
    }

    public class FpEntityContext<TEntity> : FpDbContext where TEntity : class
    {
        readonly string _tableName;
        readonly Expression<Func<TEntity, object>> _hasKey;

        public FpEntityContext(string tableName, 
            Expression<Func<TEntity, object>> hasKey = null)
        {
            _tableName = tableName;
            _hasKey = hasKey;
        }
        
        public DbSet<TEntity> FPTable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<TEntity>();
            builder.ToTable(_tableName);
            if (_hasKey != null) builder.HasKey(_hasKey);
        }
    }

    public class FpDataContext : FpEntityContext<FpData>
    {
        public FpDataContext(string tableName)
            : base(tableName, c => new { c.NO, c.IDCard })
        {
        }
    }
}