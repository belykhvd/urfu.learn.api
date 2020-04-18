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

