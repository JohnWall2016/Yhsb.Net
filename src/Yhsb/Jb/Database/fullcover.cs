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

    /// 全覆盖2省厅下发数据
    [Table("fc2_stxfsj")] public class FC2Stxfsj
    {
        public int Id { get; set; }

        /// 身份证号码
        [Key]
        public string Idcard { get; set; }
        
        /// 姓名
        public string Name { get; set; }

        /// 户籍地址
        public string Address { get; set; }

        /// 管理状态代码
        public string ManageCode { get; set; }

        /// 管理状态名称
        public string ManageName { get; set; }

        /// 是否在之前全覆盖落实总台账中 '0'-否, '1'-是
        public string InFcbooks { get; set; }

        /// 是否在全国信息比对结果中
        public string InQgbdjg { get; set; }

        /// 是否在在校学生数据中
        public string InZxxssj { get; set; }

        /// 是否在我区参加居保
        public string InSfwqjb { get; set; }

        /// 单位名称
        public string Dwmc { get; set; }

        /// 下发批次("第一批", "第二批", ...)
        public string Xfpc { get; set; }

        /// 未参保原因
        public string Wcbyy { get; set; }
    }

    /// 全覆盖2全国信息比对结果
    [Table("fc2_qgbdjg")] public class FC2Qgbdjg
    {
        [Key]
        public int Id { get; set; }

        /// 身份证号码
        public string Idcard { get; set; }
        
        /// 姓名
        public string Name { get; set; }

        /// 参保日期
        public string Cbrq { get; set; }

        /// 建立个人账户日期
        public string Jzrq { get; set; }

        /// 缴费状态
        public string Jfzt { get; set; }

        /// 断缴原因
        public string Djyy { get; set; }

        /// 行政区划
        public string Xzqh { get; set; }

        /// 数据期别
        public string Sjqb { get; set; }

        /// 险种类型
        public string Xzlx { get; set; }

        /// 备注
        public string Bz { get; set; }
    }

    /// 在校学生数据
    [Table("fc_zxxssj")] public class Zxxssj
    {
        public int Id { get; set; }

        /// 身份证号码
        public string Idcard { get; set; }
        
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
        public DbSet<FC2Stxfsj> FC2Xtxfsj { get; set; }
        public DbSet<FC2Qgbdjg> Fc2Qgbdjg { get; set; }
        public DbSet<Zxxssj> Zxxssj { get; set; }
    }
}