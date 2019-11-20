using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Yhsb.Jb.Database
{
    [Table("2019年度扶贫历史数据底册")]
    public class FPTable2019
    {
        [Column("姓名")]
        public string Name { get; set; }

        [Key, Column("身份证号码")]
        public string IDCard { get; set; }

        [Column("居保认定身份")]
        public string JBClass { get; set; }
    }

    public class JzfpContext : DbContext
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySql(_internal.Database.DBConnectString);

        public DbSet<FPTable2019> FPTable2019 { get; set; }
    }
}