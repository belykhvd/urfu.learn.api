drop table if exists user_index;
--------------------------------

create table if not exists user_index
(
    id       uuid primary key,
    fullname text not null
);

delete from user_index;
insert into user_index (id, fullname)
	values ('00000000-0000-0000-0000-000000000001', 'Saul Goodman'),
           ('00000000-0000-0000-0000-000000000002', 'Kimberly Wexler'),
           ('00000000-0000-0000-0000-000000000003', '? ?');


select * from user_index;

select ui.*
	from group_membership gm
    join user_index ui
	  on gm.user_id = ui.id
	where year = 2020
	  and semester = 8
	  and group_id = '00000000-0000-0000-0000-000000000001';