create table if not exists user_profile
(
    id uuid primary key,
    data jsonb not null
);