using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Yhsb.Jb.Database.Jzfp2020
{
    /// 扶贫数据表
    public abstract class FpData
    {
        /// 序号
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NO { get; set; }

        /// 乡镇街
        public string Xzj { get; set; }

        /// 村社区
        public string Csq { get; set; }

        /// 地址
        public string Address { get; set; }

        /// 姓名
        public string Name { get; set; }

        /// 身份证号码
        public string IDCard { get; set; }

        /// 出生日期
        public string BirthDay { get; set; }

        /// 贫困人口
        public string Pkrk { get; set; }

        /// 贫困人口日期
        public string PkrkDate { get; set; }

        /// 特困人员
        public string Tkry { get; set; }

        /// 特困人员日期
        public string TkryDate { get; set; }
        
        /// 全额低保人员
        public string Qedb { get; set; }
        
        /// 全额低保人员日期
        public string QedbDate { get; set; }
        
        /// 差额低保人员
        public string Cedb { get; set; }
        
        /// 差额低保人员日期
        public string CedbDate { get; set; }

        /// 一二级残疾人员
        public string Yejc { get; set; }

        /// 一二级残疾人员日期
        public string YejcDate { get; set; }

        /// 三四级残疾人员
        public string Ssjc { get; set; }

        /// 三四级残疾人员日期
        public string SsjcDate { get; set; }
        
        /// 属于贫困人员
        public string Sypkry { get; set; }
        
        /// 居保认定身份
        public string Jbrdsf { get; set; }
        
        /// 居保认定身份最初日期
        public string JbrdsfFirstDate { get; set; }
        
        /// 居保认定身份最后日期
        public string JbrdsfLastDate { get; set; }

        /// 居保参保情况
        public string Jbcbqk { get; set; }

        /// 居保参保情况日期
        public string JbcbqkDate { get; set; }
    }

    /// 扶贫历史数据表
    public class FpHistoryData : FpData {}

    /// 扶贫月数据表
    public class FpMonthData : FpData
    {
        /// 年月
        /// 201912
        public string Month { get; set; }
    }

    /// 扶贫原始数据表
    public class FpRawData
    {
        /// 序号
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? NO { get; set; } = null;

        /// 乡镇街
        public string Xzj { get; set; }

        /// 村社区
        public string Csq { get; set; }

        /// 地址
        public string Address { get; set; }

        /// 姓名
        public string Name { get; set; }

        /// 身份证号码
        public string IDCard { get; set; }

        /// 出生日期
        public string BirthDay { get; set; }

        /// 人员类型
        public string Type { get; set; }

        /// 类型细节
        public string Detail { get; set; }

        /// 数据月份
        public string Date { get; set; }
    }

    public class FpDbContext : DbContext
    {
        public FpDbContext() : base()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder) => 
                optionsBuilder.UseMySql(_internal.Database.DBJzfp2020);

        public DbSet<FpHistoryData> FpHistoryData { get; set; }
        public DbSet<FpMonthData> FpMonthData { get; set; }
        public DbSet<FpRawData> FpRawData { get; set; }
    }
}