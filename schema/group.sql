drop table if exists "group";
drop table if exists group_membership;
--------------------------------------

create table if not exists "group"
(
    id   uuid  primary key,
    data jsonb not null
);

delete from "group";
insert into "group" (id, data)
    values ('00000000-0000-0000-0000-000000000001', '{"id": "00000000-0000-0000-0000-000000000001", "name":" Õ-401"}'),
           ('00000000-0000-0000-0000-000000000002', '{"id": "00000000-0000-0000-0000-000000000002", "name":"‘“-401"}'),
           ('00000000-0000-0000-0000-000000000003', '{"id": "00000000-0000-0000-0000-000000000003", "name":" ¡-401"}')
    on conflict (id) do nothing;
---------------------------------------------------   
create table if not exists group_index
(
    id   uuid primary key,
    name text not null
);

insert into group_index (id, name)
	values ('00000000-0000-0000-0000-000000000001', ' Õ-401'),
           ('00000000-0000-0000-0000-000000000002', '‘“-401'),
           ('00000000-0000-0000-0000-000000000003', ' ¡-401')
	on conflict (id) do nothing;


create table if not exists group_membership
(
    year     int  not null,
    semester int  not null,
    group_id uuid not null,
    user_id  uuid not null
);

insert into group_membership (year, semester, group_id, user_id)
    values (2020, 8, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
           (2020, 8, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000002');


select * from group_membership gm ;

select * from user_index ui;

select jsonb_build_object('id', group_id, 'text', gi.name) as group,
       jsonb_agg(jsonb_build_object('id', user_id, 'text', ui.fullname)) as users
	from group_membership gm
	join group_index gi
	  on gm.group_id = gi.id
	join user_index ui
	  on gm.user_id = ui.id
	where year = 2020
	  and semester = 8
	group by group_id, gi.name;
	  
	
	


          