-------------------------------------------
create extension if not exists "uuid-ossp";
create extension if not exists "pgcrypto";
-------------------------------------------
create table if not exists auth_data
(
    email         text  primary key,
    password_hash bytea not null,
    user_id       uuid  not null
);
-------------------------------------------
create table if not exists "group"
(
	id       uuid  primary key,
	deleted  bool  not null,
	version  int   not null,
	data     jsonb not null
);

create table if not exists group_membership
(
	group_id uuid primary key,
	user_id  uuid not null
);
-------------------------------------------
create table if not exists course
(
	id       uuid  primary key,
	deleted  bool  not null,
	version  int   not null,
	data     jsonb not null
);
-------------------------------------------
create table if not exists task
(
	id       uuid  primary key,
	deleted  bool  not null,
	version  int   not null,
	data     jsonb not null
);
-------------------------------------------
create table if not exists profile
(
	id       uuid  primary key,
	deleted  bool  not null,
	version  int   not null,
	data     jsonb not null
);
-------------------------------------------