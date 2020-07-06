-- 全覆盖2省厅下发数据
CREATE TABLE IF NOT EXISTS `fullcover2020`.`fc2_stxfsj`(
    `id` INT(11) NOT NULL,
    `idcard` VARCHAR(18) NOT NULL, -- 身份证号码
    `name` VARCHAR(20) NOT NULL, -- 姓名
    `address` VARCHAR(100), -- 户籍地址
    `manage_code` VARCHAR(8), -- 管理状态代码
    `manage_name` VARCHAR(20), -- 管理状态名称
    `in_fcbooks` VARCHAR(2), -- 是否在之前全覆盖落实总台账中 '0'-否, '1'-是
    `in_qgbdjg` VARCHAR(2), -- 是否在全国信息比对结果中
    `in_zxxssj` VARCHAR(2), -- 是否在在校学生数据中
    `in_sfwqjb` VARCHAR(2), -- 是否在我区参加居保
    `dwmc` VARCHAR(100), -- 单位名称
    `xfpc` VARCHAR(20), -- 下发批次("第一批", "第二批", ...)
    `wcbyy` VARCHAR(100), -- 未参保原因
    PRIMARY KEY (`idcard`)
) DEFAULT CHARSET=utf8;

-- 全覆盖2全国信息比对结果
CREATE TABLE IF NOT EXISTS `fullcover2020`.`fc2_qgbdjg`(
    `id` INT(11) NOT NULL AUTO_INCREMENT,
    `idcard` VARCHAR(18) NOT NULL, -- 身份证号码
    `name` VARCHAR(20) NOT NULL, -- 姓名
    `cbrq` VARCHAR(20), -- 参保日期
    `jzrq` VARCHAR(20), -- 建立个人账户日期
    `jfzt` VARCHAR(50), -- 缴费状态
    `djyy` VARCHAR(50), -- 断缴原因
    `xzqh` VARCHAR(100), -- 行政区划
    `sjqb` VARCHAR(20), -- 数据期别
    `xzlx` VARCHAR(50), -- 险种类型
    `bz` VARCHAR(100), -- 备注
    PRIMARY KEY ( `id` )
) DEFAULT CHARSET=utf8;

-- 在校学生数据
CREATE TABLE IF NOT EXISTS `fullcover2020`.`fc_zxxssj`(
    `no` INT UNSIGNED, -- 序号
    `idcard` VARCHAR(18) NOT NULL, -- 身份证号码
    `name` VARCHAR(20) NOT NULL, -- 姓名
    `xz` VARCHAR(100), -- 险种
    `tcq` VARCHAR(100), -- 统筹区
    `xzj` VARCHAR(100), -- 乡镇街
    `csq` VARCHAR(100), -- 村社区
    PRIMARY KEY ( `idcard` )
) DEFAULT CHARSET=utf8;

select count(*) from `fullcover2020`.`fc2_stxfsj`;

select count(distinct idcard) from `fullcover2020`.`fc2_qgbdjg`;

update fc2_stxfsj a join fc2_qgbdjg b on a.idcard = b.idcard
set a.in_qgbdjg = '1'
where b.idcard is not null;

select count(*) from fc2_stxfsj where in_qgbdjg = '1';

update fc2_stxfsj a join fc_books b on a.idcard = b.idcard
set a.in_fcbooks = '1'
where b.idcard is not null;

select count(*) from fc2_stxfsj where in_fcbooks = '1';

update fc2_stxfsj a join fc_zxxssj b on a.idcard = b.idcard
set a.in_zxxssj = '1'
where b.idcard is not null;

select count(*) from fc2_stxfsj where in_zxxssj = '1';

update fc2_stxfsj a join jbrymx b on a.idcard = b.idcard
set a.in_sfwqjb = '1'
where b.idcard is not null;

select count(*) from fc2_stxfsj where in_sfwqjb = '1';

select count(*) from fc2_stxfsj 
where in_fcbooks <> '1' 
  and in_qgbdjg <> '1' 
  and in_zxxssj <> '1' 
  and in_sfwqjb <> '1';

select count(*) from fc2_stxfsj 
where in_fcbooks <> '1' 
  and in_qgbdjg <> '1' 
  and in_zxxssj <> '1' 
  and in_sfwqjb <> '1';

select manage_name, count(manage_name) from fc2_stxfsj 
where in_fcbooks <> '1' 
  and in_qgbdjg <> '1' 
  and in_zxxssj <> '1' 
  and in_sfwqjb <> '1'
group by manage_name;

/*
省厅下发：  263570
比对参保：  112271
第一批中：   36919
我区已参保：    36

剩下数据：  114344
公安数据显示：
  死亡：     30098
  注销：      5571
  迁出：     12939
  其它：         6
管理中：     65730
*/