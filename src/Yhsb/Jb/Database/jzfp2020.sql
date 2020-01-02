alter table fphistorydata modify idcard varchar(18);
ALTER TABLE fphistorydata DROP PRIMARY KEY, CHANGE NO NO int(11);
alter table fphistorydata drop primary key;
alter table fphistorydata add constraint PK_Idcard primary key (NO, Idcard);
alter table fphistorydata modify column NO int(11) not null AUTO_INCREMENT;

alter table fpmonthdata modify idcard varchar(18);
ALTER TABLE fpmonthdata DROP PRIMARY KEY, CHANGE NO NO int(11);
alter table fpmonthdata drop primary key;
alter table fpmonthdata add constraint PK_Idcard primary key (NO, Idcard);
alter table fpmonthdata modify column NO int(11) not null AUTO_INCREMENT;

alter table fprawdata modify idcard varchar(18);
ALTER TABLE fprawdata DROP PRIMARY KEY, CHANGE NO NO int(11);
alter table fprawdata drop primary key;
alter table fprawdata add constraint PK_Idcard primary key (NO, Idcard);
alter table fprawdata modify column NO int(11) not null AUTO_INCREMENT;

desc fphistorydata;
desc fpmonthdata;
desc fprawdata;

SELECT * FROM fphistorydata LIMIT 100;
select * from fprawdata limit 100;

select count(*) from jbrymx a left join fphistorydata b on a.idcard=b.idcard where b.idcard is null;

alter table jbrymx modify idcard varchar(18);