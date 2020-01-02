alter table fphistorydata modify idcard varchar(18);
ALTER TABLE fphistorydata DROP PRIMARY KEY, CHANGE NO NO int(11);
alter table fphistorydata add constraint PK_NO primary key (NO);
alter table fphistorydata modify column NO int(11) not null AUTO_INCREMENT;
alter table fphistorydata drop index UNQ_Idcard;
alter table fphistorydata add constraint UNQ_Idcard unique (Idcard);
desc fphistorydata;

alter table fpmonthdata modify idcard varchar(18);
ALTER TABLE fpmonthdata DROP PRIMARY KEY, CHANGE NO NO int(11);
alter table fpmonthdata add constraint PK_NO primary key (NO);
alter table fpmonthdata modify column NO int(11) not null AUTO_INCREMENT;
alter table fpmonthdata drop index UNQ_Idcard;
alter table fpmonthdata modify month varchar(6);
alter table fpmonthdata add constraint UNQ_Idcard unique (Idcard, month);
desc fpmonthdata;

alter table fprawdata modify idcard varchar(18);
ALTER TABLE fprawdata DROP PRIMARY KEY, CHANGE NO NO int(11);
alter table fprawdata add constraint PK_NO primary key (NO);
alter table fprawdata modify column NO int(11) not null AUTO_INCREMENT;
alter table fprawdata modify date varchar(6);
alter table fprawdata modify type varchar(100);
alter table fprawdata add constraint UNQ_Idcard unique (Idcard, date, type);
desc fprawdata;

SELECT * FROM fphistorydata LIMIT 100;
select * from fprawdata limit 100;

select count(*) from jbrymx a left join fphistorydata b on a.idcard=b.idcard where b.idcard is null;

alter table jbrymx modify idcard varchar(18);

select count(*) from fpmonthdata;

select count (*) from fprawdata;

select * from fpmonthdata where pkrk is not null and tkry is not null;
select * from fpmonthdata where idcard='43031119641215101X';