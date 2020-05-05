create table if not exists task
(
	id uuid primary key,
	data jsonb not null
);

create table if not exists task_index
(
	id uuid primary key,
	name text not null
);