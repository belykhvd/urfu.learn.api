create table if not exists course
(
	id uuid primary key,
	data jsonb not null
);

create table if not exists course_index
(
	id uuid primary key,
	name text not null
);