create table if not exists solution_info
(
	id   uuid  primary key,
	data jsonb not null
);

create table if not exists solution_index
(
	id           uuid primary key,
	challenge_id uuid not null,
	user_id      uuid not null,
	number       int  not null
);

insert into solution_info (id, data)
	values ('00000000000000000000000000000001', '{"Id": "00000000000000000000000000000001", "FileName": "попытка1.zip", "FileSize": 1024, "Timestamp": "2020-04-18T22:00:00.1748438+05:00"}'),
 	       ('00000000000000000000000000000002', '{"Id": "00000000000000000000000000000002", "FileName": "попытка2.zip", "FileSize": 1025, "Timestamp": "2020-04-18T22:22:00.1748438+05:00"}')
	on conflict (id) do nothing;

insert into solution_index (id, challenge_id, user_id, number)
	values ('00000000000000000000000000000001', '00000000000000000000000000000001', '00000000000000000000000000000001', 1),
	       ('00000000000000000000000000000002', '00000000000000000000000000000001', '00000000000000000000000000000001', 2)
	on conflict (id) do nothing;

select inf.data
	from solution_index si
	join solution_info inf
	  on si.id = inf.id
	where user_id = '00000000000000000000000000000001'
	  and challenge_id = '00000000000000000000000000000001'
	order by number desc;
	  
	  

