create table if not exists task
(
	id uuid primary key,
	data jsonb not null
);

create table if not exists task_index
(
	id uuid primary key,
	name text not null,
	max_score int not null default 0,
	requirements jsonb
);

create table if not exists task_progress
(
	user_id uuid not null,
	task_id uuid not null,
	score int not null default 0,
	done uuid[]
);