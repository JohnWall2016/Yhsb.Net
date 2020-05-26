CREATE DATABASE IF NOT EXISTS fullcover2020 DEFAULT CHARACTER SET utf8 DEFAULT COLLATE utf8_general_ci;

-- 全覆盖已下发数据
CREATE TABLE IF NOT EXISTS `fullcover2020`.`fc_yxfsj`(
   `dwmc` VARCHAR(100) NOT NULL, -- 单位名称
   `xfpc` VARCHAR(20) NOT NULL, -- 下发批次("第一批", "第二批", ...)
   `no` INT UNSIGNED, -- 序号
   `idcard` VARCHAR(18) NOT NULL, -- 身份证号码
   `name` VARCHAR(20) NOT NULL, -- 姓名
   `xz` VARCHAR(100), -- 险种
   `tcq` VARCHAR(100), -- 统筹区
   `xzj` VARCHAR(100), -- 乡镇街
   `csq` VARCHAR(100), -- 村社区
   `sfycb` VARCHAR(10), -- 是否已参保("是", "否")
   `cbsj` VARCHAR(20), -- 参保时间
   `shsj` VARCHAR(20), -- 审核时间
   `wcbyy` VARCHAR(100), -- 未参保原因
   PRIMARY KEY ( `idcard` )
) DEFAULT CHARSET=utf8;

-- 全覆盖落实总台账
CREATE TABLE IF NOT EXISTS `fullcover2020`.`fc_books`(
   `dwmc` VARCHAR(100) NOT NULL, -- 单位名称
   `idcard` VARCHAR(18) NOT NULL, -- 身份证号码
   `name` VARCHAR(20) NOT NULL, -- 姓名
   `address` VARCHAR(100), -- 地址
   `hsqk` VARCHAR(50) NOT NULL, -- 核实情况
   PRIMARY KEY (`idcard`)
) DEFAULT CHARSET=utf8;

-- 居保参保人员明细表
CREATE TABLE IF NOT EXISTS `fullcover2020`.`jbrymx`(
   `idcard` VARCHAR(18) NOT NULL, -- 身份证号码
   `xzqh` VARCHAR(100), -- 行政区划
   `hjxz` VARCHAR(10), -- 户籍性质
   `name` VARCHAR(20) NOT NULL, -- 姓名
   `sex` VARCHAR(10) NOT NULL, -- 性别
   `birthday` VARCHAR(10) NOT NULL, -- 出生日期
   `cbsf` VARCHAR(10) NOT NULL, -- 参保身份
   `cbzt` VARCHAR(10) NOT NULL, -- 参保状态
   `jfzt` VARCHAR(10) NOT NULL, -- 缴费状态
   `cbsj` VARCHAR(20) NOT NULL, -- 参保时间
   PRIMARY KEY (`idcard`)
) DEFAULT CHARSET=utf8;

SELECT * FROM `fullcover2020`.`fc_yxfsj` where idcard='430321196405261520';
SELECT * FROM `fullcover2020`.`fc_yxfsj` where idcard='430302200006273065';
SELECT * FROM `fullcover2020`.`fc_yxfsj` where idcard='430321200103172214';
SELECT * FROM `fullcover2020`.`fc_yxfsj` where idcard='430321200104162210';

select dwmc, count(dwmc) from `fullcover2020`.`fc_yxfsj` group by dwmc;

select count(*) from `fullcover2020`.`fc_yxfsj` where xfpc = '第一批';

select count(*) from `fullcover2020`.`fc_yxfsj` where xfpc = '第二批';

select count(*) from `fullcover2020`.`fc_yxfsj` where xfpc = '第三批';

SELECT * FROM `fullcover2020`.`fc_yxfsj` where idcard='430302200408240047';

SELECT * FROM `fullcover2020`.`fc_yxfsj` where idcard='430302199011234778';

SELECT * FROM `fullcover2020`.`fc_books` where dwmc='广场街道';

SELECT * FROM `fullcover2020`.`fc_books` where idcard='430321200111102216';

SELECT count(*) FROM `fullcover2020`.`fc_books`;
SELECT count(*) FROM `fullcover2020`.`fc_yxfsj`;
SELECT count(*) FROM `fullcover2020`.`jbrymx`;

SELECT * FROM `fullcover2020`.`jbrymx` LIMIT 100;

SELECT count(*) FROM `fullcover2020`.`fc_yxfsj` where dwmc='先锋街道';

SELECT count(*) FROM `fullcover2020`.`fc_yxfsj` where Sfycb='是';

SELECT count(*) FROM `fullcover2020`.`fc_yxfsj` where wcbyy is not null and wcbyy<>'';

SELECT * FROM `fullcover2020`.`fc_yxfsj` LIMIT 100;

SELECT count(*) FROM `fullcover2020`.`fc_yxfsj` where wcbyy='' and Sfycb<>'是';

SELECT * FROM `fullcover2020`.`fc_yxfsj` where wcbyy='' and Sfycb<>'是' order by ;

update`fullcover2020`.`fc_yxfsj` set wcbyy='16岁以上在校生' where wcbyy='16岁以上在校学生';

update`fullcover2020`.`fc_yxfsj` set wcbyy='参职保（含退休）' where wcbyy='参职保(含退休)';

update`fullcover2020`.`fc_yxfsj` set wcbyy='服刑人员' where wcbyy='服刑';

update`fullcover2020`.`fc_yxfsj` set wcbyy='已录入居保' where wcbyy='已参居保';

select a.*, b.name as jbname from `fullcover2020`.`fc_yxfsj` as a, `fullcover2020`.`jbrymx` as b where a.idcard=b.idcard and a.name<>b.name;

update `fullcover2020`.`jbrymx` set name='张新伟' where idcard='43030319680412001X';
