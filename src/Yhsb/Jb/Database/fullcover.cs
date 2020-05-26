using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Yhsb.Jb.Database.FullCover2020
{
    /// 已下发数据
    [Table("fc_yxfsj")] public class Yxfsj
    {
        /// 单位名称
        public string Dwmc { get; set; }

        /// 下发批次("第一批", "第二批", ...)
        public string Xfpc { get; set; }

        /// 序号
        public int No { get; set; }

        /// 身份证号码
        [Key] public string Idcard { get; set; }

        /// 姓名
        public string Name { get; set; }

        /// 险种
        public string Xz { get; set; }

        /// 统筹区
        public string Tcq { get; set; }

        /// 乡镇街
        public string Xzj { get; set; }

        /// 村社区
        public string Csq { get; set; }

        /// 是否已参保("是", "否")
        public string Sfycb { get; set; }

        /// 参保时间
        public string Cbsj { get; set; }

        /// 审核时间
        public string Shsj { get; set; }

        /// 未参保原因
        public string Wcbyy { get; set; }
    }

    /// 落实总台账
    [Table("fc_books")] public class Books
    {
        /// 单位名称
        public string Dwmc { get; set; }

        /// 身份证号码
        [Key] public string Idcard { get; set; }

        /// 姓名
        public string Name { get; set; }

        /// 地址
        public string Address { get; set; }

        /// 核实情况
        public string Hsqk { get; set; }
    }

    /// 居保参保人员明细表
    public class Jbrymx
    {
        /// 身份证号码
        [Key]
        public string Idcard { get; set; }

        /// 行政区划
        public string Xzqh { get; set; }

        /// 户籍性质
        public string Hjxz { get; set; }

        /// 姓名
        public string Name { get; set; }

        /// 性别
        public string Sex { get; set; }

        /// 出生日期
        public string BirthDay { get; set; }

        /// 参保身份
        public string Cbsf { get; set; }

        /// 参保状态
        public string Cbzt { get; set; }

        /// 缴费状态
        public string Jfzt { get; set; }

        /// 参保时间
        public string Cbsj { get; set; }
    }
    
    public class Context : DbContext
    {
        public Context() : base()
        {
            // Database.EnsureCreated();
        }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder) =>
                optionsBuilder.UseMySql(_internal.Database.FullCover2020);

        public DbSet<Yxfsj> Yxfsjs { get; set; }
        public DbSet<Books> Books { get; set; }
        public DbSet<Jbrymx> Jbrymx { get; set; }
    }
}