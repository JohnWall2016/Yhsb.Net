using System;
using System.IO;
using System.Text;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Yhsb.Util.Excel;

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

    [Table("2019年度扶贫办民政残联历史数据")]
    public class FpRawData2019 : FpRawData
    {
    }

    [Table("2019年度扶贫历史数据底册")]
    public class FpData2019 : FpData
    {
    }

    public class FpDbContext : DbContext
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder) => 
                optionsBuilder.UseMySql(_internal.Database.DBConnectString);
        
        public DbSet<FpRawData2019> FpRawData2019 { get; set; }

        public DbSet<FpData2019> FpData2019 { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FpRawData2019>()
                .HasKey(c => new { c.NO, c.IDCard });

            modelBuilder.Entity<FpData2019>()
                .HasKey(c => new { c.NO, c.IDCard });
        }

        public int LoadExcel(
            string tableName, string excelFile, int startRow, int endRow,
            string[] fields, HashSet<string> noQuote = null)
        {
            var workbook = ExcelExtension.LoadExcel(excelFile);
            var sheet = workbook.GetSheetAt(0);

            var buf = new StringBuilder();
            for (var index = startRow; index <= endRow; index++)
            {
                var values = new List<string>();
                foreach (var row in fields)
                {
                    var value = sheet.GetRow(index).Cell(row).Value();
                    if (noQuote != null && noQuote.Contains(row))
                        value = $"'{value}'";
                    values.Add(value);
                }
                buf.Append(string.Join(",", values));
                buf.Append("\n");
            }

            var tmpFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tmpFile, buf.ToString());
                var loadSql =
                    $"load data infile '{tmpFile.Replace(@"\", @"\\")}' into table `{tableName}` " +
                    "CHARACTER SET utf8 FIELDS TERMINATED BY ',' " +
                    "OPTIONALLY ENCLOSED BY '\\'' LINES TERMINATED BY '\\n'";
                return Database.ExecuteSqlRaw(loadSql);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }
    }

    public class FpDbContextWith<TEntity> : FpDbContext
        where TEntity : class
    {
        public DbSet<TEntity> Entity { get; set; }

        readonly string _tableName;

        readonly Expression<Func<TEntity, object>> _keyExpression;

        public FpDbContextWith(
            string tableName, 
            Expression<Func<TEntity, object>> keyExpression = null)
        {
            _tableName = tableName;
            _keyExpression = keyExpression;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<TEntity>();
            builder.ToTable(_tableName);
            if (_keyExpression != null)
                builder.HasKey(_keyExpression);
        }
    }
}