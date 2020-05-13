CREATE DATABASE IF NOT EXISTS fullcover2020 DEFAULT CHARACTER SET utf8 DEFAULT COLLATE utf8_general_ci;

-- 全覆盖已下发数据
CREATE TABLE IF NOT EXISTS `fc_yxfsj`(
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

SELECT * FROM `fullcover2020`.`fc_yxfsj` where idcard='430321196405261520';
SELECT * FROM `fullcover2020`.`fc_yxfsj` where idcard='430302200006273065';
SELECT * FROM `fullcover2020`.`fc_yxfsj` where idcard='430321200103172214';
SELECT * FROM `fullcover2020`.`fc_yxfsj` where idcard='430321200104162210';

select dwmc, count(dwmc) from `fullcover2020`.`fc_yxfsj` group by dwmc;

select count(*) from `fullcover2020`.`fc_yxfsj` where xfpc = '第一批';